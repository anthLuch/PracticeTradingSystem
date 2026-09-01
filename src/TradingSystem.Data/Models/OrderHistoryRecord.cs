using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingSystem.Data.Models
{
    public readonly record struct OrderHistoryRecord(
    long Id,
    string Symbol,
    string Side,
    string OrderType,
    decimal Price,
    long OriginalQuantity,
    long Quantity,
    string Status,
    DateTime CreatedAt
    );
}
