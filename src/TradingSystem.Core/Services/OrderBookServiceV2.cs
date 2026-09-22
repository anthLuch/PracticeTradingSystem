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

        private readonly int _scale;
        private readonly int _maxPrice;

        private readonly LinkedList<Order>?[] _bids;
        private readonly LinkedList<Order>?[] _asks;

        private int _bestBid = -1;
        private int _bestAsk = -1;
        private int priceScale = 0;


        public decimal PriceIncrement { get; }
        public decimal? BestBid => _bestBid == -1 ? null : (decimal)_bestBid / _scale;
        public decimal? BestAsk => _bestAsk == -1 ? null : (decimal)_bestAsk / _scale;

        private readonly Dictionary<long, LinkedListNode<Order>> _orderIndex;

        private long _nextSequence = 0;

        public OrderBookServiceV2(IMatchingEngineServiceV2 matchingEngine, decimal priceIncrement)
        {
            _matchingEngine = matchingEngine;
            PriceIncrement = priceIncrement;
            _scale = (int)Math.Round(1m / priceIncrement);
            _maxPrice = 100_000 * _scale;

            _bids = new LinkedList<Order>?[_maxPrice + 1];
            _asks = new LinkedList<Order>?[_maxPrice + 1];

            _orderIndex = new Dictionary<long, LinkedListNode<Order>>();
        }

        private int _orderCount = 0;

        public int OrderCount => _orderCount;

        public List<Trade> Submit(Order order)
        {
            priceScale = (int)(order.Price * _scale);
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

            if (book[priceScale] == null)
            {
                book[priceScale] = new LinkedList<Order>();
            }

            LinkedListNode<Order> node = book[priceScale].AddLast(order);
            _orderIndex.Add(order.Id, node);

            if (order.Side == Side.Buy)
            {
                if(_bestBid == -1 || priceScale > _bestBid)
                {
                    _bestBid = priceScale;
                }
            }
            else
            {
                if (_bestAsk == -1 || priceScale < _bestAsk)
                {
                    _bestAsk = priceScale;
                }
            }
        }

        public Order GetOrder(long orderId)
        {
            return _orderIndex.TryGetValue(orderId, out LinkedListNode<Order> node) ? node.Value: null;
        }

        public bool Cancel(long Id)
        {
            if (!_orderIndex.ContainsKey(Id))
            {
                return false;
            }

            LinkedListNode<Order> order = _orderIndex[Id];
            priceScale = (int)(order.Value.Price * _scale);

            if (order.Value.Side == Side.Buy)
            {
                _bids[priceScale].Remove(order);
                if (_bids[priceScale].Count == 0) _bids[priceScale] = null;
                if (priceScale == _bestBid && _bids[priceScale] == null)
                {
                    int scan = _bestBid - 1;
                    _bestBid = -1;
                    while (scan >= 0)
                    {
                        if (_bids[scan] != null) { _bestBid = scan; break; }
                        scan--;
                    }
                }
            }
            else
            {
                _asks[priceScale].Remove(order);
                if (_asks[priceScale].Count == 0) _asks[priceScale] = null;
                if (priceScale == _bestAsk && _asks[priceScale] == null)
                {
                    int scan = _bestAsk + 1;
                    _bestAsk = -1;
                    while (scan <= _maxPrice)
                    {
                        if (_asks[scan] != null) { _bestAsk = scan; break; }
                        scan++;
                    }
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
                for(int i = _maxPrice; i >= 0; i--)
                {
                    if (count >= depth) yield break;
                    if (_bids[i] == null) continue;

                    long totalq = _bids[i].Sum(o => o.Quantity);
                    yield return ((decimal)i / _scale, totalq);
                    count++;
                }
            }
            else
            {
                for (int i = 0; i <= _maxPrice; i++)
                {
                    if (count >= depth) yield break;
                    if (_asks[i] == null) continue;

                    long totalQ = _asks[i].Sum(o => o.Quantity);
                    yield return ((decimal)i / _scale, totalQ);
                    count++;
                }

            }

        }


    }
}

