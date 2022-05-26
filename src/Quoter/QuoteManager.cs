using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Quoter
{
    public class QuoteManager : IQuoteManager
    {
        private readonly ILogger<QuoteManager> _logger;
        public ConcurrentDictionary<string, ConcurrentDictionary<Guid, IQuote>> QuotesBySymbol { get; private set; }

        public QuoteManager(ILogger<QuoteManager> logger)
        {
            _logger = logger;
            QuotesBySymbol = new ConcurrentDictionary<string, ConcurrentDictionary<Guid, IQuote>>();
        }

        public void AddOrUpdateQuote(IQuote quote)
        {
            ConcurrentDictionary<Guid, IQuote> quotes;
            if (!QuotesBySymbol.TryGetValue(quote.Symbol, out quotes))
            {
                quotes = new ConcurrentDictionary<Guid, IQuote>();
                QuotesBySymbol.TryAdd(quote.Symbol, quotes);
            }

            quotes.AddOrUpdate(quote.Id, quote, (id, oldQuote) => quote); 
        }

        // I would have added this as part of a separate Execution Manager
        // which would have access to the QuoteManager/QuoteRepository
        public ITradeResult ExecuteTrade(string symbol, uint volumeRequested)
        {
            _logger.LogInformation("Starting Execution of - Symbol: {Symbol} | VolumeRequested: {VolumeRequested}", symbol, volumeRequested);

            var orderedQuotes = GetOrderedQuotes(symbol);

            // If no quotes then no trades
            if (orderedQuotes == null || !orderedQuotes.Any())
            {
                _logger.LogInformation("No quotes found for trade {Symbol}", symbol);
                return null;
            }

            // Make trades, best quotes first
            var weightSum = 0d;
            var weightedValueSum = 0d;
            var remainingToExecute = volumeRequested;
            var trade = new TradeResult
            {
                Id = Guid.NewGuid(),
                Symbol = symbol,
                VolumeExecuted = 0,
                VolumeRequested = volumeRequested,
                VolumeWeightedAveragePrice = 0
            };

            // Intentionnaly decided not to use GetBestQuoteWithAvailableVolume for performance purpose
            foreach (var quote in orderedQuotes)
            {
                if (remainingToExecute == 0)
                {
                    break;
                }

                var volumeToExecute = remainingToExecute >= quote.AvailableVolume ? quote.AvailableVolume : remainingToExecute;
                trade.VolumeExecuted += volumeToExecute;
                quote.AvailableVolume -= volumeToExecute;
                remainingToExecute -= volumeToExecute;

                weightSum += volumeToExecute;
                weightedValueSum += quote.Price * volumeToExecute;

                _logger.LogInformation("Executed Symbol: {Symbol} | Volume: {Volume} | Price: {Price}",
                    symbol,
                    volumeToExecute,
                    quote.Price);
            }

            trade.VolumeWeightedAveragePrice = weightedValueSum / weightSum;

            _logger.LogInformation("Completed Execution of {@Trade}", trade);

            return trade;
        }

        public IQuote GetBestQuoteWithAvailableVolume(string symbol)
        {
            var bestQuote = GetOrderedQuotes(symbol)?
                .FirstOrDefault();

            return bestQuote;
        }

        public void RemoveAllQuotes(string symbol)
        {
            QuotesBySymbol.TryRemove(symbol, out var _); // O(1)
        }

        public void RemoveQuote(Guid id)
        {
            foreach (var quotes in QuotesBySymbol.Values) // O(n)
            {
                if (quotes.TryGetValue(id, out var _))
                {
                    quotes.TryRemove(id, out var _);
                    return;
                }
            }
        }

        private IOrderedEnumerable<IQuote> GetOrderedQuotes(string symbol)
        {
            QuotesBySymbol.TryGetValue(symbol, out var quotes);
            var orderedQuotes = quotes?.Values?
                .Where(quote => quote.AvailableVolume > 0 &&
                                quote.ExpirationDate > DateTime.Now)? // O(n)
                .OrderBy(quote => quote.Price); // O(log(n))

            return orderedQuotes;
        }
    }
}

