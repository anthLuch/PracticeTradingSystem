using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingSystem.Data.Models
{
    public readonly record struct TradeHistoryRecord(
        long TradeId,
        long BuyOrderId,
        long SellOrderId,
        string Symbol,
        decimal Price,
        long Quantity,
        DateTime ExecutedAt        
        );
}
