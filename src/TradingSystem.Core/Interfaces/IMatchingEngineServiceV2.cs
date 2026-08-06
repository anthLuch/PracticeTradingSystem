using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingSystem.Core.Models;

namespace TradingSystem.Core.Interfaces
{
    public interface IMatchingEngineServiceV2
    {
        List<Trade> Match(Order order,
            LinkedList<Order>?[] bids,
            LinkedList<Order>?[] asks, ref int bestBid, ref int bestAsk, ref int orderCount);

    }
}
