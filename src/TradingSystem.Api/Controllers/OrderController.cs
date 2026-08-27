using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TradingSystem.Api.DTO;
using TradingSystem.Api.Services;
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
        private readonly OrderRepositoryProcessor _orderRepositoryProcessor;
        private readonly IOrderRepository _orderRepository;
        private readonly ITradeRespository _tradeRespository;

        private static long _nextId = 0;

        public OrderController(IOrderBookService orderBookService, PositionTracker positionTracker, OrderBookProcessing orderBookProcessing,
                                IOrderRepository orderRepository, ITradeRespository tradeRespository, OrderRepositoryProcessor orderRepositoryProcessor)
        {
            _orderBookService = orderBookService;
            _positionTracker = positionTracker;
            _orderBookProcessing = orderBookProcessing;
            _orderRepository = orderRepository;
            _tradeRespository = tradeRespository;
            _orderRepositoryProcessor = orderRepositoryProcessor;

        }

        [HttpPost]
        public async Task<ActionResult> SubmitOrderRequest([FromBody] OrderRequest request)
        {
            List<Trade> trades = new List<Trade>();
            List<Order> updatedRestingOrders = new List<Order>();
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

                foreach (Trade trade in trades)
                {
                    Order restingOrder = null;
                    long id = order.Side == Side.Buy ? trade.SellOrderId : trade.BuyOrderId;

                    restingOrder = _orderBookService.GetOrder(id);

                    updatedRestingOrders.Add(restingOrder);
                    
                    _positionTracker.ProcessTrade(trade, order.Side);
                }

                await _orderRepositoryProcessor.QueueAsync(order, trades, updatedRestingOrders);

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
