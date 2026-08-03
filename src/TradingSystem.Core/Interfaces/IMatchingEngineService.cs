using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingSystem.Core.Models;

namespace TradingSystem.Core.Interfaces
{
    public interface IMatchingEngineService
    {
        List<Trade> Match(Order order,
            SortedDictionary<decimal, LinkedList<Order>> bids,
            SortedDictionary<decimal, LinkedList<Order>> asks, ref int orderCount);

    }
}
