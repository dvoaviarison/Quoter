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
        public IActionResult AddOrUpdateQuote([FromBody] Quote quote)
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

        [HttpPost("RemoveAllQuotes")]
        public IActionResult RemoveAllQuotes(string symbol)
        {
            try
            {
                _logger.LogInformation("[BEGIN] Calling RemoveAllQuotes");

                _quoteManager.RemoveAllQuotes(symbol);

                _logger.LogInformation("[End] RemoveAllQuotes completed successfully");

                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "[END] RemoveAllQuotes failed. {ExceptionMessage}", e.Message);
                return BadRequest(e.Message);
            }
        }

        [HttpPost("RemoveQuote")]
        public IActionResult RemoveQuote(Guid id)
        {
            try
            {
                _logger.LogInformation("[BEGIN] Calling RemoveQuote");

                _quoteManager.RemoveQuote(id);

                _logger.LogInformation("[End] RemoveQuote completed successfully");

                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "[END] RemoveQuote failed. {ExceptionMessage}", e.Message);
                return BadRequest(e.Message);
            }
        }
    }
}

