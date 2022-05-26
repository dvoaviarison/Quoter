using System;

namespace Quoter
{
    public class Quote : IQuote
    {
        public Guid Id { get; set; }
        public string Symbol { get; set; }
        public double Price { get; set; }
        public uint AvailableVolume { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}

