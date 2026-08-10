using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using TradingSystem.Core.Interfaces;
using TradingSystem.Core.Messaging;
using TradingSystem.Core.Models;

namespace TradingSystem.Core.Services
{
    public class OrderBookProcessing
    {
        private readonly IOrderBookService _orderBookService;
        private readonly Channel<OrderRequest> _channel;

        public OrderBookProcessing(IOrderBookService orderBookService)
        {
            _orderBookService = orderBookService;
            _channel = Channel.CreateUnbounded<OrderRequest>();

        }

        public async Task<List<Trade>> SubmitAsync(Order order)
        {
            OrderRequest request = new OrderRequest(order);

            await _channel.Writer.WriteAsync(request);
            return await request.Result.Task;
        } 

        public async Task StartAsync(CancellationToken ct = default)
        {
            await foreach(OrderRequest request in _channel.Reader.ReadAllAsync(ct))
            {
                List<Trade> trades = _orderBookService.Submit(request.Order);

               request.Result.SetResult(trades);

            }
        }

    }
}
