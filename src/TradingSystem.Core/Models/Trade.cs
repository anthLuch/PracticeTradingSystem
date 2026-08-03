using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingSystem.Core.Models
{
    public readonly record struct Trade(
        long TradeId,
        long BuyOrderId,
        long SellOrderId,
        decimal Price,
        long Quantity
    );

}