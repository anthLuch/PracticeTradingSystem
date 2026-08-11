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
        private const int MaxPrice = 100_000;
        private long _nextTradeId = 1;
        public List<Trade> Match(Order order, LinkedList<Order>?[] bids,
            LinkedList<Order>?[] asks, ref int bestBid, ref int bestAsk, ref int orderCount)
        {
            List<Trade> trades = new List<Trade>();

            if(order.OrderType == OrderType.FOK)
            {
                long available = 0;
                if (order.Side == Side.Buy)
                {

                    for (int i = bestAsk; i <= order.Price && i <= MaxPrice; i++)
                    {
                        if (asks[i] == null) continue;
                        foreach(Order ord in asks[i])
                        {
                            available += ord.OriginalQuantity;
                            if(available > ord.Quantity) break;
                        }                      
                    }

                }
                else
                {
                    for (int i = bestBid; i >= order.Price && i != 0; i++)
                    {
                        if (bids[i] == null) continue;
                        foreach (Order ord in bids[i])
                        {
                            available += ord.OriginalQuantity;
                            if (available > ord.Quantity) break;
                        }
                    }

                }
                if (available >= order.OriginalQuantity) return new List<Trade>();
            }

            switch (order.Side)
            {
                case Side.Buy:

                    while (order.Quantity > 0 && bestAsk != -1 && bestAsk <= MaxPrice && (order.OrderType == OrderType.Market || order.Price >= (decimal)bestAsk))
                    {
                        var bestLevel = asks[bestAsk];
                        if (bestLevel == null) break;

                        Order ord = bestLevel.First.Value;

                        long matchingQuantity = Math.Min(ord.Quantity, order.Quantity);

                        ord.Filled(matchingQuantity);
                        order.Filled(matchingQuantity);

                        trades.Add(new Trade(
                            TradeId: _nextTradeId++,
                            BuyOrderId: order.Id,
                            SellOrderId: ord.Id,
                            Price: ord.Price,
                            Quantity: matchingQuantity
                            ));

                        if (ord.IsFilled)
                        {
                            bestLevel.RemoveFirst();
                            orderCount--;
                        }

                        if (bestLevel.Count == 0)
                        {
                            asks[bestAsk] = null;
                            while (bestAsk <= MaxPrice && asks[bestAsk] == null)
                            {
                                bestAsk++;
                            }
                        }

                        if (order.Quantity == 0) break;
                    }
                    break;
                case Side.Sell:

                    while (order.Quantity > 0 && bestBid != -1 && bestBid >= 0 && (order.OrderType == OrderType.Market || order.Price <= (decimal)bestBid))
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
                            Price: ord.Price,
                            Quantity: matchingQuantity
                            ));

                        if (ord.IsFilled)
                        {
                            bestLevel.RemoveFirst();
                            orderCount--;
                        }

                        if (bestLevel.Count == 0)
                        {
                            bids[bestBid] = null;
                            while (bestBid >= 0 && asks[bestBid] == null)
                            {
                                bestBid--;
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
