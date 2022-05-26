using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Quoter.Tests
{
    public class QuoteManagerTests
    {
        [Theory]
        [InlineData("SEC1", 10, 0, false, 20, 2000, false, 20)]
        [InlineData("SEC1", 10, 1000, true, 20, 2000, false, 20)]
        [InlineData("SEC1", 10, 1000, false, 20, 2000, false, 10)]
        [InlineData("SEC1", 10, 0, false, 20, 0, false, null)]
        [InlineData("SEC1", 10, 1000, true, 20, 2000, true, null)]
        [InlineData("SEC2", 10, 1000, false, 20, 2000, false, null)]
        public void CanGetBestQuoteWithAvailableVolume(
            string secName,
            double quote1Price,
            uint quote1AvailableVolume,
            bool quote1IsExpired,
            double quote2Price,
            uint quote2AvailableVolume,
            bool quote2IsExpired,
            double? expectedBestPrice)
        {
            // GIVEN
            var mgr = new QuoteManager(new NullLogger<QuoteManager>());
            var quote1 = new Quote
            {
                Id = Guid.NewGuid(),
                Symbol = "SEC1",
                Price = quote1Price,
                AvailableVolume = quote1AvailableVolume,
                ExpirationDate = DateTime.Now.AddDays(quote1IsExpired ? -2 : 2)
            };
            var quote2 = new Quote
            {
                Id = Guid.NewGuid(),
                Symbol = "SEC1",
                Price = quote2Price,
                AvailableVolume = quote2AvailableVolume,
                ExpirationDate = DateTime.Now.AddDays(quote2IsExpired ? -2 : 2)
            };
            mgr.QuotesBySymbol[quote1.Symbol] = new ConcurrentDictionary<Guid, IQuote> { [quote1.Id] = quote1 };
            mgr.QuotesBySymbol[quote2.Symbol].TryAdd(quote2.Id, quote2);

            // WHEN
            var bestQuote = mgr.GetBestQuoteWithAvailableVolume(secName);

            // THEN
            Assert.Equal(expectedBestPrice, bestQuote?.Price);
        }

        [Theory]
        [InlineData("SEC1", 1500, 1500, (double)2000/1500, 0, 1500)]
        [InlineData("SEC1", 3500, 3000, (double)5000/3000, 0, 0)]
        [InlineData("SEC2", 3500, 0, 0, 1000, 2000)]
        public void CanExecuteTrade(
            string symbol,
            uint requestedVolume,
            uint expectedVolumeExecuted,
            double expectedAvgPrice,
            uint quote1RemainingVolume,
            uint quote2RemainingVolume)
        {
            // GIVEN
            var mgr = new QuoteManager(new NullLogger<QuoteManager>());
            var quote1 = new Quote
            {
                Id = Guid.NewGuid(),
                Symbol = "SEC1",
                Price = 1,
                AvailableVolume = 1000,
                ExpirationDate = DateTime.Now.AddDays(2)
            };
            var quote2 = new Quote
            {
                Id = Guid.NewGuid(),
                Symbol = "SEC1",
                Price = 2,
                AvailableVolume = 2000,
                ExpirationDate = DateTime.Now.AddDays(2)
            };
            mgr.QuotesBySymbol[quote1.Symbol] = new ConcurrentDictionary<Guid, IQuote> { [quote1.Id] = quote1 };
            mgr.QuotesBySymbol[quote2.Symbol].TryAdd(quote2.Id, quote2);

            // WHEN
            var res = mgr.ExecuteTrade(symbol, requestedVolume);

            // THEN
            Assert.Equal(expectedVolumeExecuted, res?.VolumeExecuted ?? 0);
            Assert.Equal(expectedAvgPrice, res?.VolumeWeightedAveragePrice ?? 0);
            Assert.Equal(quote1RemainingVolume, mgr.QuotesBySymbol[quote1.Symbol][quote1.Id].AvailableVolume);
            Assert.Equal(quote2RemainingVolume, mgr.QuotesBySymbol[quote2.Symbol][quote2.Id].AvailableVolume);
        }

        [Fact]
        public void CanAddQuote()
        {
            // GIVEN
            var mgr = new QuoteManager(new NullLogger<QuoteManager>());
            var quote = new Quote
            {
                Id = Guid.NewGuid(),
                Symbol = "SEC1",
                Price = 10,
                AvailableVolume = 1000,
                ExpirationDate = DateTime.Now.AddDays(2)
            };

            // WHEN
            mgr.AddOrUpdateQuote(quote);

            // THEN
            Assert.Single(mgr.QuotesBySymbol);
            Assert.Single(mgr.QuotesBySymbol[quote.Symbol]);
            Assert.Equal(quote.Price, mgr.QuotesBySymbol[quote.Symbol][quote.Id].Price);
            Assert.Equal(quote.AvailableVolume, mgr.QuotesBySymbol[quote.Symbol][quote.Id].AvailableVolume);
            Assert.Equal(quote.ExpirationDate, mgr.QuotesBySymbol[quote.Symbol][quote.Id].ExpirationDate);
        }

        [Fact]
        public void CanUpdateQuote()
        {
            // GIVEN
            var mgr = new QuoteManager(new NullLogger<QuoteManager>());
            var oldQuote = new Quote
            {
                Id = Guid.NewGuid(),
                Symbol = "SEC1",
                Price = 10,
                AvailableVolume = 1000,
                ExpirationDate = DateTime.Now.AddDays(2)
            };
            mgr.QuotesBySymbol[oldQuote.Symbol] = new ConcurrentDictionary<Guid, IQuote> { [oldQuote.Id] = oldQuote };
            var newQuote = new Quote
            {
                Id = oldQuote.Id,
                Symbol = "SEC1",
                Price = 15,
                AvailableVolume = 700,
                ExpirationDate = DateTime.Now.AddDays(4)
            };

            // WHEN
            mgr.AddOrUpdateQuote(newQuote);

            // THEN
            Assert.Single(mgr.QuotesBySymbol);
            Assert.Single(mgr.QuotesBySymbol[newQuote.Symbol]);
            Assert.Equal(newQuote.Price, mgr.QuotesBySymbol[oldQuote.Symbol][oldQuote.Id].Price);
            Assert.Equal(newQuote.AvailableVolume, mgr.QuotesBySymbol[oldQuote.Symbol][oldQuote.Id].AvailableVolume);
            Assert.Equal(newQuote.ExpirationDate, mgr.QuotesBySymbol[oldQuote.Symbol][oldQuote.Id].ExpirationDate);
        }

        [Fact]
        public void CanRemoveAllQuotes()
        {
            // GIVEN
            var mgr = new QuoteManager(new NullLogger<QuoteManager>());
            var quote = new Quote
            {
                Id = Guid.NewGuid(),
                Symbol = "SEC1",
                Price = 10,
                AvailableVolume = 1000,
                ExpirationDate = DateTime.Now.AddDays(2)
            };
            mgr.QuotesBySymbol[quote.Symbol] = new ConcurrentDictionary<Guid, IQuote> { [quote.Id] = quote };

            // WHEN
            mgr.RemoveAllQuotes(quote.Symbol);

            // THEN
            Assert.False(mgr.QuotesBySymbol.TryGetValue(quote.Symbol, out var _));
        }

        [Fact]
        public void CanRemoveQuote()
        {
            // GIVEN
            var mgr = new QuoteManager(new NullLogger<QuoteManager>());
            var quote1 = new Quote
            {
                Id = Guid.NewGuid(),
                Symbol = "SEC1",
                Price = 10,
                AvailableVolume = 1000,
                ExpirationDate = DateTime.Now.AddDays(2)
            };
            var quote2 = new Quote
            {
                Id = Guid.NewGuid(),
                Symbol = "SEC1",
                Price = 20,
                AvailableVolume = 2000,
                ExpirationDate = DateTime.Now.AddDays(3)
            };
            mgr.QuotesBySymbol[quote1.Symbol] = new ConcurrentDictionary<Guid, IQuote> { [quote1.Id] = quote1 };
            mgr.QuotesBySymbol[quote2.Symbol].TryAdd(quote2.Id, quote2);

            // WHEN
            mgr.RemoveQuote(quote1.Id);

            // THEN
            Assert.Single(mgr.QuotesBySymbol[quote2.Symbol]);
            Assert.Equal(quote2.Id, mgr.QuotesBySymbol[quote2.Symbol].Values.First().Id);
        }
    }
}

