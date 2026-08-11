using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingSystem.Core.Interfaces;
using TradingSystem.Core.Models;

namespace TradingSystem.Core.Services
{
    public class OrderBookServiceV2 : IOrderBookService
    {
        private readonly IMatchingEngineServiceV2 _matchingEngine;
        private readonly PositionTracker _positionTracker = new PositionTracker();

        private const int MaxPrice = 100_000;

        private readonly LinkedList<Order>?[] _bids = new LinkedList<Order>?[MaxPrice + 1];
        private readonly LinkedList<Order>?[] _asks = new LinkedList<Order>?[MaxPrice + 1];

        private int _bestBid = -1;
        private int _bestAsk = -1;

        public decimal? BestBid => _bestBid == -1 ? null : (decimal)_bestBid;
        public decimal? BestAsk => _bestAsk == -1 ? null : (decimal)_bestAsk;

        private readonly Dictionary<long, LinkedListNode<Order>> _orderIndex;

        private long _nextSequence = 0;
        public OrderBookServiceV2(IMatchingEngineServiceV2 matchingEngine)
        {
            _matchingEngine = matchingEngine;

            _orderIndex = new Dictionary<long, LinkedListNode<Order>>();
        }

        private int _orderCount = 0;

        public int OrderCount => _orderCount;

        public List<Trade> Submit(Order order)
        {

            order.Sequence = _nextSequence++;

            List<Trade> trades = _matchingEngine.Match(order, _bids, _asks, ref _bestBid, ref _bestAsk, ref _orderCount);

            foreach(Trade trade in trades)
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

            if (book[(int)order.Price] == null)
            {
                book[(int)order.Price] = new LinkedList<Order>();
            }

            LinkedListNode<Order> node = book[(int)order.Price].AddLast(order);
            _orderIndex.Add(order.Id, node);

            if (order.Side == Side.Buy)
            {
                if(_bestBid == -1 || (int)order.Price > _bestBid)
                {
                    _bestBid = (int)order.Price;
                }
            }
            else
            {
                if (_bestAsk == -1 || (int)order.Price < _bestAsk)
                {
                    _bestAsk = (int)order.Price;
                }
            }
        }

        public bool Cancel(long Id)
        {
            if (!_orderIndex.ContainsKey(Id))
            {
                return false;
            }

            LinkedListNode<Order> order = _orderIndex[Id];

            if (order.Value.Side == Side.Buy)
            {
                _bids[(int)order.Value.Price].Remove(order);
                if (_bids[(int)order.Value.Price].Count == 0) _bids[(int)order.Value.Price] = null;
                if ((int)order.Value.Price == _bestBid)
                {
                    while (_bestBid >= 0 && _bids[_bestBid] == null)
                        _bestBid--;
                }
            }
            else
            {
                _asks[(int)order.Value.Price].Remove(order);
                if (_asks[(int)order.Value.Price].Count == 0) _asks[(int)order.Value.Price] = null;
                if ((int)order.Value.Price == _bestAsk)
                {
                    while (_bestAsk <= MaxPrice && _asks[_bestAsk] == null)
                        _bestAsk++;
                }
            }

            _orderIndex.Remove(Id);
            _orderCount--;
            return true;
        }

        public IEnumerable<(decimal price, long quantity)> GetDepth(Side side, int depth)
        {
            var book = side == Side.Buy ? _bids : _asks;
            int count = 0;

            if(side == Side.Buy)
            {
                for(int i = MaxPrice; i >= 0; i--)
                {
                    if (count >= depth) yield break;
                    if (_bids[i] == null) continue;

                    long totalq = _bids[i].Sum(o => o.Quantity);
                    yield return ((decimal)i, totalq);
                    count++;
                }
            }
            else
            {
                for (int i = 0; i <= MaxPrice; i++)
                {
                    if (count >= depth) yield break;
                    if (_asks[i] == null) continue;

                    long totalQ = _asks[i].Sum(o => o.Quantity);
                    yield return ((decimal)i, totalQ);
                    count++;
                }

            }

        }


    }
}

