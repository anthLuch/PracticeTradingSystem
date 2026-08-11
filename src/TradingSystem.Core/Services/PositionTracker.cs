using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingSystem.Core.Models;

namespace TradingSystem.Core.Services
{
    public class PositionTracker
    {
        private decimal _position = 0;
        private decimal _averageEntryPrice = 0;
        private decimal _realisedPL = 0;
        private decimal _unrealisedPL = 0;
        public decimal Position => _position;
        public decimal AverageEntryPrice => _averageEntryPrice;
        public decimal RealisedPL => _realisedPL;
        public decimal UnrealisedPL => _unrealisedPL;

        public void ProcessTrade(Trade trade, Side side)
        {
            if(side == Side.Buy)
            {
                decimal oldPosition = _position;
                _position += trade.Quantity;
                _averageEntryPrice = ((_averageEntryPrice *  oldPosition) + (trade.Price * trade.Quantity)) / _position;
            }
            else
            {
                _position -= trade.Quantity;
                _realisedPL += (trade.Price - _averageEntryPrice) * trade.Quantity;
            }

            _unrealisedPL = _position * (trade.Price - _averageEntryPrice);
        }
    }
}
