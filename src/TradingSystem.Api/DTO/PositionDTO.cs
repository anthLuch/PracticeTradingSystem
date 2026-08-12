using Microsoft.AspNetCore.Mvc;

namespace TradingSystem.Api.DTO
{
    public class PositionDTO
    {
        public decimal Position { get; set;}
        public decimal AverageEntryPrice { get; set; }
        public decimal RealisedPL { get; set; }
        public decimal UnrealisedPL { get; set; }

    }
}
