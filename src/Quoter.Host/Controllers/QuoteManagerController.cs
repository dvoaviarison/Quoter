using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Quoter.Host.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QuoteManagerController : ControllerBase
    {
        private readonly ILogger<QuoteManagerController> _logger;
        private readonly IQuoteManager _quoteManager;

        public QuoteManagerController(
            ILoggerFactory loggerFactory,
            IQuoteManager quoteManager)
        {
            _logger = loggerFactory.CreateLogger<QuoteManagerController>();
            _quoteManager = quoteManager;
        }

        [HttpPost("ExecuteTrade")]
        public ActionResult<ITradeResult> ExecuteTrade(string symbol, uint volumeRequested)
        {
            try
            {
                _logger.LogInformation("[BEGIN] Calling ExecuteTrade");

                var res = _quoteManager.ExecuteTrade(symbol, volumeRequested);

                _logger.LogInformation("[End] ExecuteTrade completed successfully");

                return Ok(res);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "[END] ExecuteTrade failed. {ExceptionMessage}", e.Message);
                return BadRequest(e.Message);
            }
        }

        [HttpPost("AddOrUpdateQuote")]
        public IActionResult AddOrUpdateQuote([FromBody] IQuote quote)
        {
            try
            {
                _logger.LogInformation("[BEGIN] Calling AddOrUpdateQuote");

                _quoteManager.AddOrUpdateQuote(quote);

                _logger.LogInformation("[End] AddOrUpdateQuote completed successfully");

                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "[END] AddOrUpdateQuote failed. {ExceptionMessage}", e.Message);
                return BadRequest(e.Message);
            }
        }
    }
}

