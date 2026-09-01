using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace TradingSystem.Data.EfCore
{
    public class TradeEntity
    {
        [Key]
        public int TradeId { get; set ; }

        public int BuyOrderId { get; set ; }

        public int SellOrderId { get; set ; }

        public string Symbol { get; set ; }

        public decimal Price { get; set ; }

        public int Quantity { get; set ; }

        public DateTime ExecutedAt { get; set ; }


    }
}
