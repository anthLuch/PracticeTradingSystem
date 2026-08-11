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
    public class MatchingEngineService : IMatchingEngineService
    {

        private long _nextTradeId = 1;
        public List<Trade> Match(Order order, SortedDictionary<decimal, LinkedList<Order>> bids,
            SortedDictionary<decimal, LinkedList<Order>> asks, ref int orderCount)
        {
            List<Trade> trades = new List<Trade>();

            if (order.OrderType == OrderType.FOK)
            {

                long available = 0;
                if (order.Side == Side.Buy)
                {
                    foreach(KeyValuePair<decimal, LinkedList<Order>> level in asks)
                    {
                        if (level.Key > order.Price) break;
                        foreach (Order ord in level.Value)
                        {
                            available += ord.Quantity;
                            if (available >= order.OriginalQuantity) break;
                        }
                    }
                }
                else
                {
                    foreach (KeyValuePair<decimal, LinkedList<Order>> level in bids)
                    {
                        if (level.Key < order.Price) break;
                        foreach (Order ord in level.Value)
                        {
                            available += ord.Quantity;
                            if (available >= order.OriginalQuantity) break;
                        }
                    }
                }

                if (available < order.OriginalQuantity) return new List<Trade>();

            }

            switch (order.Side)
            {
                case Side.Buy:

                    while (order.Quantity > 0 && asks.Count > 0 && (order.OrderType == OrderType.Market || order.Price >= asks.First().Key))
                    {
                        var bestLevel = asks.First();

                        Order ord = bestLevel.Value.First.Value;

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
                            bestLevel.Value.RemoveFirst();
                            orderCount--;
                        }

                        if (bestLevel.Value.Count == 0)
                        {
                            asks.Remove(bestLevel.Key);
                        }

                        if (order.Quantity == 0) break;
                    }
                    break;
                case Side.Sell:

                    while (order.Quantity > 0 && bids.Count > 0 && (order.OrderType == OrderType.Market || order.Price <= bids.First().Key))
                    {
                        var bestLevel = bids.First();

                        Order ord = bestLevel.Value.First.Value;

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
                            bestLevel.Value.RemoveFirst();
                            orderCount--;
                        }

                        if (bestLevel.Value.Count == 0)
                        {
                            bids.Remove(bestLevel.Key);
                        }

                        if (order.Quantity == 0) break;
                    }
                    break;
            }

            return trades;
        }
    }
}
