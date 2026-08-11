using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingSystem.Core.Interfaces;
using TradingSystem.Core.Models;

namespace TradingSystem.Core.Services
{
    public class OrderBookService : IOrderBookService
    {
        private readonly IMatchingEngineService _matchingEngine;
        private readonly SortedDictionary<decimal, LinkedList<Order>> _bids;

        private readonly SortedDictionary<decimal, LinkedList<Order>> _asks;

        private readonly Dictionary<long, LinkedListNode<Order>> _orderIndex;

        private readonly PositionTracker _positionTracker = new PositionTracker();

        private long _nextSequence = 0;
        public OrderBookService(IMatchingEngineService matchingEngine)
        {
            _matchingEngine = matchingEngine;

            _bids = new SortedDictionary<decimal, LinkedList<Order>>(Comparer<decimal>.Create((a, b) => b.CompareTo(a)));

            _asks = new SortedDictionary<decimal, LinkedList<Order>>();

            _orderIndex = new Dictionary<long, LinkedListNode<Order>>();
        }


        public decimal? BestBid => _bids.Count == 0 ? null : _bids.Keys.First();

        public decimal? BestAsk => _asks.Count == 0 ? null : _asks.Keys.First();

        private int _orderCount = 0;

        public int OrderCount => _orderCount;

        public List<Trade> Submit(Order order)
        {
            
            order.Sequence = _nextSequence++;

            List<Trade> trades = _matchingEngine.Match(order, _bids, _asks, ref _orderCount);

            foreach (Trade trade in trades)
            {
                _positionTracker.ProcessTrade(trade, order.Side);
            }

            if (!order.IsFilled && order.OrderType == OrderType.Limit)
            {
                restOrder(order);
                _orderCount++;
            }

            return trades;
        }

        private void restOrder(Order order)
        {
            var book = order.Side == Side.Buy ? _bids : _asks;

            if (!book.ContainsKey(order.Price))
            {
                book[order.Price] = new LinkedList<Order>();
            }

            LinkedListNode<Order> node = book[order.Price].AddLast(order);
            _orderIndex.Add(order.Id, node);
        }

        public bool Cancel(long Id)
        {
            if(!_orderIndex.ContainsKey(Id))
            {
                return false;
            }

            LinkedListNode<Order> order = _orderIndex[Id];

            if(order.Value.Side == Side.Buy)
            {
                _bids[order.Value.Price].Remove(order);
                if (_bids[order.Value.Price].Count == 0) _bids.Remove(order.Value.Price);
            }
            else
            {
                _asks[order.Value.Price].Remove(order);
                if (_asks[order.Value.Price].Count == 0) _asks.Remove(order.Value.Price);
            }

            _orderIndex.Remove(Id);
            _orderCount--;
            return true;
        }

        public IEnumerable<(decimal price, long quantity)> GetDepth(Side side, int depth)
        {
            var book = side == Side.Buy ? _bids : _asks;
            int count = 0;

            foreach(var levels in book)
            {
                if(count >= depth) yield break;

                long totalQ = 0;
                foreach( Order level in levels.Value)
                {
                    totalQ += level.Quantity;
                }

                yield return (levels.Key, totalQ);
                count++;
            }
        }


    }
}
