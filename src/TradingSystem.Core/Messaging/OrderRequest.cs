using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingSystem.Core.Models;

namespace TradingSystem.Core.Messaging
{
    public class OrderRequest
    {
        public Order Order {  get; }
        public TaskCompletionSource<List<Trade>> Result {  get; }
        public OrderRequest(Order order) 
        {
            Order = order;
            Result = new TaskCompletionSource<List<Trade>>();
        }
    }
}
