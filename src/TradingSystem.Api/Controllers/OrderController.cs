using Microsoft.AspNetCore.Mvc;
using TradingSystem.Api.DTO;
using TradingSystem.Core.Interfaces;
using TradingSystem.Core.Models;
using TradingSystem.Core.Services;

namespace TradingSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderBookService _orderBookService;
        private readonly PositionTracker _positionTracker;
        private static long _nextId = 0;

        public OrderController(IOrderBookService orderBookService, PositionTracker positionTracker)
        {
            _orderBookService = orderBookService;
            _positionTracker = positionTracker;
        }

        [HttpPost]
        public ActionResult SubmitOrderRequest([FromBody] OrderRequest request)
        {
            List<Trade> trades = new List<Trade>();
            Order order = new Order(
                id: Interlocked.Increment(ref _nextId),
                side: request.Side,
                price: request.Price,
                orderType: request.OrderType,
                originalQuantity: request.OriginalQuantity
                );

            try
            {
                trades = _orderBookService.Submit(order);

                foreach(Trade trade in trades)
                {
                    _positionTracker.ProcessTrade(trade, order.Side);
                }
                
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

            return Ok(trades);
        }

        [HttpDelete("{orderId}")]
        public ActionResult CancelOrder(long orderId)
        {
            try
            {
                if(!_orderBookService.Cancel(orderId))
                {
                    return NotFound("Order was unable to be cancelled");
                }
            }
            catch( Exception ex ) 
            {
                return StatusCode(500, ex.Message);
            }

            return Ok("Order Cancelled succesfully");
        }

        [HttpGet("depth")]
        public ActionResult GetDepth(Side side, int levels)
        {
            IEnumerable<(decimal price, long level)> results = null;
            try
            {
                results =  _orderBookService.GetDepth(side, levels);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

            return Ok(results.Select(r => new DepthLevel { Price = r.price, Quantity = r.level }).ToList());
        }

        [HttpGet("position")]
        public ActionResult GetPosition()
        {
            PositionResponse pos = new PositionResponse
            {
                Position = _positionTracker.Position,
                AverageEntryPrice = _positionTracker.AverageEntryPrice,
                RealisedPL = _positionTracker.RealisedPL,
                UnrealisedPL = _positionTracker.UnrealisedPL,
            };

            return Ok(pos);
        }

    }
}
