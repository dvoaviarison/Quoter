using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Quoter.Tests
{
    public class QuoteManagerTests
    {
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

