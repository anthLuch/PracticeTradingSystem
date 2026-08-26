using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TradingSystem.Api.DTO;
using TradingSystem.Core.Interfaces;
using TradingSystem.Core.Models;
using TradingSystem.Core.Services;
using TradingSystem.Data.Interfaces;

namespace TradingSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderBookService _orderBookService;
        private readonly PositionTracker _positionTracker;
        private readonly OrderBookProcessing _orderBookProcessing;
        private readonly IOrderRepository _orderRepository;
        private readonly ITradeRespository _tradeRespository;

        private static long _nextId = 0;

        public OrderController(IOrderBookService orderBookService, PositionTracker positionTracker, OrderBookProcessing orderBookProcessing, IOrderRepository orderRepository, ITradeRespository tradeRespository)
        {
            _orderBookService = orderBookService;
            _positionTracker = positionTracker;
            _orderBookProcessing = orderBookProcessing;
            _orderRepository = orderRepository;
            _tradeRespository = tradeRespository;

        }

        [HttpPost]
        public async Task<ActionResult> SubmitOrderRequest([FromBody] OrderRequest request)
        {
            List<Trade> trades = new List<Trade>();
            Order order = new Order(
                id: Interlocked.Increment(ref _nextId),
                side: request.Side,
                price: request.Price,
                symbol: "AAPL",
                orderType: request.OrderType,
                originalQuantity: request.OriginalQuantity,
                createdAt: DateTime.UtcNow
                );

            try
            {
                trades = await _orderBookProcessing.SubmitAsync(order);

                await _orderRepository.InsertAsync(order);

                foreach (Trade trade in trades)
                {
                    Order updatedOrder = null;
                    if(order.Side == Side.Buy)
                    {
                        updatedOrder = _orderBookService.GetOrder(trade.SellOrderId);
                    }
                    else
                    {
                        updatedOrder = _orderBookService.GetOrder(trade.BuyOrderId);
                    }

                    await _orderRepository.UpdateAsync(updatedOrder.Id, updatedOrder.Quantity, updatedOrder.IsFilled);
                    await _tradeRespository.InsertAsync(trade);
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
        public async Task<ActionResult> CancelOrder(long orderId)
        {
            try
            {
                if(!_orderBookService.Cancel(orderId))
                {
                    return NotFound("Order was unable to be cancelled");
                }

                await _orderRepository.UpdateStatusAsync(orderId, "Cancelled");
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
