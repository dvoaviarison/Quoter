using System;

namespace Quoter
{
    public class QuoteManager : IQuoteManager
    {
        public void AddOrUpdateQuote(IQuote quote)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void RemoveQuote(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}

