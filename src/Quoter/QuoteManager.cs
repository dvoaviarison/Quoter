using System;
using System.Collections.Concurrent;
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
                QuotesBySymbol[quote.Symbol] = quotes;
            }

            quotes.AddOrUpdate(quote.Id, quote, (id, oldQuote) => quote);
        }

        public ITradeResult ExecuteTrade(string symbol, uint volumeRequested)
        {
            throw new NotImplementedException();
        }

        public IQuote GetBestQuoteWithAvailableVolume(string symbol)
        {
            throw new NotImplementedException();
        }

        public void RemoveAllQuotes(string symbol)
        {
            // Catch if something goes wrong here
            QuotesBySymbol.TryRemove(symbol, out var _);
        }

        public void RemoveQuote(Guid id)
        {
            foreach (var quotes in QuotesBySymbol.Values)
            {
                if (quotes.TryGetValue(id, out var _))
                {
                    quotes.TryRemove(id, out var _);
                    return;
                }
            }
        }
    }
}

