using System;
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
            var quote = new Quote
            {
                Id = Guid.NewGuid(),
                Symbol = "SEC1",
                Price = 10,
                AvailableVolume = 1000,
                ExpirationDate = DateTime.Now.AddDays(2)
            };
            var newQuote = new Quote
            {
                Id = quote.Id,
                Symbol = "SEC1",
                Price = 15,
                AvailableVolume = 700,
                ExpirationDate = DateTime.Now.AddDays(4)
            };

            // WHEN
            mgr.AddOrUpdateQuote(quote);
            mgr.AddOrUpdateQuote(newQuote);

            // THEN
            Assert.Single(mgr.QuotesBySymbol);
            Assert.Single(mgr.QuotesBySymbol[newQuote.Symbol]);
            Assert.Equal(newQuote.Price, mgr.QuotesBySymbol[quote.Symbol][quote.Id].Price);
            Assert.Equal(newQuote.AvailableVolume, mgr.QuotesBySymbol[quote.Symbol][quote.Id].AvailableVolume);
            Assert.Equal(newQuote.ExpirationDate, mgr.QuotesBySymbol[quote.Symbol][quote.Id].ExpirationDate);
        }
    }
}

