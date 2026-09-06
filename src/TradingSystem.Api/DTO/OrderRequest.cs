using Microsoft.AspNetCore.Mvc;
using TradingSystem.Core.Models;

namespace TradingSystem.Api.DTO
{
    public class OrderRequest
    {
        public Side Side { get; set; }

        public string Symbol { get; set; }

        public OrderType OrderType { get; set; }

        public decimal Price { get; set; }

        public long OriginalQuantity { get; set; }
    }
}
