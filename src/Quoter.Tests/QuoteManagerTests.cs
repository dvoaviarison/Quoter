using Xunit;

namespace Quoter.Tests
{
    public class QuoteManagerTests
    {
        [Fact]
        public void CanExecuteTrade()
        {
            // GIVEN
            var mgr = new QuoteManager();

            // WHEN
            var res = mgr.ExecuteTrade("SEC", 1000);

            // THEN
            Assert.NotNull(res);
        }
    }
}

