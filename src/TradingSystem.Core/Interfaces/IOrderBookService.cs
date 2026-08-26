using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingSystem.Core.Models;

namespace TradingSystem.Core.Interfaces
{
    public interface IOrderBookService
    {
        decimal? BestBid { get; }

        decimal? BestAsk { get; }

        int OrderCount { get; }

        List<Trade> Submit(Order order);

        Order? GetOrder(long orderId);

        bool Cancel(long orderId);

        IEnumerable<(decimal price, long quantity)> GetDepth(Side side, int levels);



    }
}
