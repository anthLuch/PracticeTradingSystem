using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TradingSystem.Core.Models;
using TradingSystem.Data.EfCore;
using TradingSystem.Data.Interfaces;
using TradingSystem.Data.Models;
using TradingSystem.Data.Repositories;

namespace TradingSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TradeController : ControllerBase
    {
        private readonly ITradeRespository _tradeRespository;
        private readonly ITradeSummaryRepository _tradeSummaryRepository;
        public TradeController(ITradeRespository tradeRespository, ITradeSummaryRepository tradeSummaryRepository)
        {
            _tradeRespository = tradeRespository;
            _tradeSummaryRepository = tradeSummaryRepository;
        }


        [HttpGet("history")]
        public async Task<ActionResult> GetTradeHistory(string? symbol, int? limit)
        {
            IEnumerable<TradeHistoryRecord> result = null;
            try
            {
                result = await _tradeRespository.GetAllHistoryAsync(symbol, limit);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

            return Ok(result);
        }

        [HttpGet("summary")]
        public async Task<ActionResult> GetTradesSummary()
        {
            IEnumerable<TradeSummaryRecord> result = null;
            try
            {
                result = await _tradeSummaryRepository.GetAllTradesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

            return Ok(result);
        }
    }
}
