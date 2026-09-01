using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingSystem.Data.Models
{
    public class TradeSummaryRecord
    {
        public string Symbol { get; set; }
        public long TotalQuantity { get; set; }

        public decimal AveragePrice { get; set; }

        public int TradeCount { get; set; }

    }
}
