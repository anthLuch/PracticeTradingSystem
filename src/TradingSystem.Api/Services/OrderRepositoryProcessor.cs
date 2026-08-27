using System.Threading.Channels;
using System.Threading.Tasks;
using TradingSystem.Api.Models;
using TradingSystem.Core.Models;
using TradingSystem.Data.Interfaces;

namespace TradingSystem.Api.Services
{
    public class OrderRepositoryProcessor
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ITradeRespository _tradeRepository;
        private readonly Channel<OrderPersistenceRequest> _channel;

        public OrderRepositoryProcessor(IOrderRepository orderRepository, ITradeRespository tradeRepository) 
        {
            _orderRepository = orderRepository;
            _tradeRepository = tradeRepository;
            _channel = Channel.CreateUnbounded<OrderPersistenceRequest>();
        }

        public async Task QueueAsync(Order order, List<Trade> trades, List<Order> existingOrders)
        {
            OrderPersistenceRequest request = new OrderPersistenceRequest(order, trades, existingOrders);

            await _channel.Writer.WriteAsync(request);
        }

        public async Task StartAsync(CancellationToken ct=default)
        {
            await foreach(OrderPersistenceRequest request in _channel.Reader.ReadAllAsync(ct))
            {
                try
                {
                    await _orderRepository.InsertAsync(request.SubmittedOrder);

                    foreach (Trade trade in request.Trades)
                    {
                        await _tradeRepository.InsertAsync(trade);
                    }

                    foreach (Order order in request.RestingOrders)
                    {
                        await _orderRepository.UpdateAsync(order.Id, order.Quantity, order.IsFilled);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Failed to persist order {request.SubmittedOrder.Id}: {ex.Message}");
                }

            }

        }
    }
}
