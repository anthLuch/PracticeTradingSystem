using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TradingSystem.Core.Interfaces;
using TradingSystem.Core.Models;

namespace TradingSystem.Core.Services
{
    public class MatchingEngineServiceV2 : IMatchingEngineServiceV2
    {
        private readonly int _scale;
        private readonly int _maxPrice;

        public MatchingEngineServiceV2(decimal priceIncrement)
        {
            _scale = (int)Math.Round(1m / priceIncrement);
            _maxPrice = 100_000 * _scale;
        }
        private long _nextTradeId = 1;
        public List<Trade> Match(Order order, LinkedList<Order>?[] bids,
            LinkedList<Order>?[] asks, ref int bestBid, ref int bestAsk, ref int orderCount)
        {
            List<Trade> trades = new List<Trade>();

            if (order.OrderType == OrderType.FOK)
            {
                long available = 0;
                if (order.Side == Side.Buy)
                {
                    if(bestAsk != -1)
                    {
                        for (int i = bestAsk; i <= order.Price * _scale && i <= _maxPrice; i++)
                        {
                            if (asks[i] == null) continue;
                            foreach (Order ord in asks[i])
                            {
                                available += ord.Quantity;
                                if (available >= order.OriginalQuantity) break;
                            }
                        }
                    }
                }
                else
                {
                    for (int i = bestBid; i >= order.Price * _scale && i != 0; i--)
                    {
                        if (bids[i] == null) continue;
                        foreach (Order ord in bids[i])
                        {
                            available += ord.Quantity;
                            if (available > order.OriginalQuantity) break;
                        }
                    }

                }
                if (available < order.OriginalQuantity) return new List<Trade>();
            }

            switch (order.Side)
            {
                case Side.Buy:

                    while (order.Quantity > 0 && bestAsk != -1 && bestAsk <= _maxPrice && (order.OrderType == OrderType.Market || order.Price >= (decimal)bestAsk / _scale))
                    {
                        LinkedList<Order> bestLevel = asks[bestAsk];
                        if (bestLevel == null) break;

                        Order ord = bestLevel.First.Value;

                        long matchingQuantity = Math.Min(ord.Quantity, order.Quantity);

                        ord.Filled(matchingQuantity);
                        order.Filled(matchingQuantity);

                        trades.Add(new Trade(
                            TradeId: _nextTradeId++,
                            BuyOrderId: order.Id,
                            SellOrderId: ord.Id,
                            Symbol: order.Symbol,
                            Price: ord.Price,
                            Quantity: matchingQuantity,
                            ExecutedAt: DateTime.UtcNow
                            ));

                        if (ord.IsFilled)
                        {
                            bestLevel.RemoveFirst();
                            orderCount--;
                        }

                        if (bestLevel.Count == 0)
                        {
                            asks[bestAsk] = null;
                            int scan = bestAsk + 1;
                            bestAsk = -1;
                            while (scan <= _maxPrice)
                            {
                                if (asks[scan] != null) { bestAsk = scan; break; }
                                scan++;
                            }
                        }

                        if (order.Quantity == 0) break;
                    }
                    break;
                case Side.Sell:

                    while (order.Quantity > 0 && bestBid != -1 && bestBid >= 0 && (order.OrderType == OrderType.Market || order.Price <= (decimal)bestBid / _scale))
                    {
                        if (bestBid == -1) break;
                        var bestLevel = bids[bestBid];
                        if (bestLevel == null) break;

                        Order ord = bestLevel.First.Value;

                        long matchingQuantity = Math.Min(ord.Quantity, order.Quantity);

                        ord.Filled(matchingQuantity);
                        order.Filled(matchingQuantity);

                        trades.Add(new Trade(
                            TradeId: _nextTradeId++,
                            BuyOrderId: ord.Id,
                            SellOrderId: order.Id,
                            Symbol: order.Symbol,
                            Price: ord.Price,
                            Quantity: matchingQuantity,
                            ExecutedAt: DateTime.UtcNow
                            ));

                        if (ord.IsFilled)
                        {
                            bestLevel.RemoveFirst();
                            orderCount--;
                        }

                        if (bestLevel.Count == 0)
                        {
                            bids[bestBid] = null;
                            int scan = bestBid - 1;
                            bestBid = -1;
                            while (scan >= 0)
                            {
                                if (bids[scan] != null) { bestBid = scan; break; }
                                scan--;
                            }
                        }

                        if (order.Quantity == 0) break;
                    }
                    break;
            }

            return trades;
        }
    }
}
