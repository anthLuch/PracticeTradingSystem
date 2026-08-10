using System.Diagnostics;
using System.Drawing;
using TradingSystem.Core.Interfaces;
using TradingSystem.Core.Models;
using TradingSystem.Core.Services;
using Xunit;

namespace TradingSystem.Tests;

public class OrderBookTests
{
    // A buy order with no resting asks should rest in the book untouched
    [Fact]
    public void NoMatch_WhenBookIsEmpty_OrderRests()
    {
        IMatchingEngineService matchingEngineService = new MatchingEngineService();
        IOrderBookService orderBookService = new OrderBookService(matchingEngineService);

        Order order = new Order(id: 1, side: Side.Buy, price: 101m, originalQuantity: 5);


        List<Trade> trades = orderBookService.Submit(order);

        Assert.Empty(trades);
        Assert.Equal(101m, orderBookService.BestBid);
        Assert.Equal(1, orderBookService.OrderCount);
        Assert.Null(orderBookService.BestAsk);

    }

    // Ensures selling price and buying price work.
    [Fact]
    public void FullMatch_ExactQuantity_ProducesOneTrade()
    {
        IMatchingEngineService matchingEngineService = new MatchingEngineService();
        IOrderBookService orderBookService = new OrderBookService(matchingEngineService);

        Order orderSell = new Order(id: 1, side: Side.Sell, price: 99m, originalQuantity: 5);
        orderBookService.Submit(orderSell);
        Order orderBuy = new Order(id: 2, side: Side.Buy, price: 101m, originalQuantity: 5);

        List<Trade> trades = orderBookService.Submit(orderBuy);

        Assert.Equal(1, trades.Count);
        Assert.Equal(99m, trades[0].Price);
        Assert.Equal(5, trades[0].Quantity);

        Assert.Null(orderBookService.BestBid);       
        Assert.Null(orderBookService.BestAsk);        
        Assert.Equal(0, orderBookService.OrderCount); 

    }

    // A resting sell with quantity 10 and an incoming bid of 4 ends with a trade of 4
    [Fact]
    public void PartialFill_LeavesRemainderResting()
    {
        IMatchingEngineService matchingEngineService = new MatchingEngineService();
        IOrderBookService orderBookService = new OrderBookService(matchingEngineService);

        Order orderSell = new Order(id: 1, side: Side.Sell, price: 101m, originalQuantity: 10);
        orderBookService.Submit(orderSell);
        Order orderBuy = new Order(id: 2, side: Side.Buy, price: 101m, originalQuantity: 4);

        List<Trade> trades = orderBookService.Submit(orderBuy);

        Assert.Equal(1, trades.Count);
        Assert.Equal(101m, trades[0].Price);
        Assert.Equal(4, trades[0].Quantity);

        Assert.Null(orderBookService.BestBid);
        Assert.Equal(101m, orderBookService.BestAsk);
        Assert.Equal(1, orderBookService.OrderCount);
    }


    // Checks for FIFO ensuring the first in at the same price is sold
    [Fact]
    public void SamePrice_OlderOrderTradesFirst()
    {
        IMatchingEngineService matchingEngineServiceV2 = new MatchingEngineService();
        IOrderBookService orderBookServiceV2 = new OrderBookService(matchingEngineServiceV2);

        orderBookServiceV2.Submit(new Order(id: 1, side: Side.Sell, price: 101m, originalQuantity: 5));
        orderBookServiceV2.Submit(new Order(id: 2, side: Side.Sell, price: 101m, originalQuantity: 10));
        Order orderBuy = new Order(id: 3, side: Side.Buy, price: 101m, originalQuantity: 5);

        List<Trade> trades = orderBookServiceV2.Submit(orderBuy);

        Assert.Equal(1, trades.Count);
        Assert.Equal(101m, trades[0].Price);
        Assert.Equal(1, trades[0].SellOrderId);
        Assert.Equal(5, trades[0].Quantity);
        Assert.Equal(1, orderBookServiceV2.OrderCount);
    }

    // Ensures prices are sold at the best price they can make
    [Fact]
    public void AggressorGetsRestingPrice_NotOwnLimitPrice()
    {
        IMatchingEngineService matchingEngineService = new MatchingEngineService();
        IOrderBookService orderBookService = new OrderBookService(matchingEngineService);

        Order orderSell = new Order(id: 1, side: Side.Sell, price: 99m, originalQuantity: 5);
        orderBookService.Submit(orderSell);
        Order orderBuy = new Order(id: 2, side: Side.Buy, price: 101m, originalQuantity: 5);

        List<Trade> trades = orderBookService.Submit(orderBuy);

        Assert.Equal(1, trades.Count);
        Assert.Equal(99m, trades[0].Price);
        Assert.Equal(5, trades[0].Quantity);

        Assert.Null(orderBookService.BestBid);
        Assert.Null(orderBookService.BestAsk);
        Assert.Equal(0, orderBookService.OrderCount);
    }

    // Checking not fully fullied buys move onto the next existing sell price
    [Fact]
    public void OrderWalksMultiplePriceLevels()
    {
        IMatchingEngineService matchingEngineService = new MatchingEngineService();
        IOrderBookService orderBookService = new OrderBookService(matchingEngineService);

        orderBookService.Submit(new Order(id: 1, side: Side.Sell, price: 100m, originalQuantity: 5));
        orderBookService.Submit(new Order(id: 2, side: Side.Sell, price: 101m, originalQuantity: 10));
        Order orderBuy = new Order(id: 3, side: Side.Buy, price: 101m, originalQuantity: 8);

        List<Trade> trades = orderBookService.Submit(orderBuy);

        Assert.Equal(2, trades.Count);
        Assert.Equal(100m, trades[0].Price);
        Assert.Equal(101m, trades[1].Price);
        Assert.Equal(1, trades[0].SellOrderId);
        Assert.Equal(5, trades[0].Quantity);
        Assert.Equal(2, trades[1].SellOrderId);
        Assert.Equal(3, trades[1].Quantity);
        Assert.Equal(1, orderBookService.OrderCount);
    }

    // A bid at 99 and an ask at 101 (no overlap) should produce zero trades,
    [Fact]
    public void NonCrossingOrders_BothRest_NoTrades()
    {
        IMatchingEngineService matchingEngineService = new MatchingEngineService();
        IOrderBookService orderBookService = new OrderBookService(matchingEngineService);

        orderBookService.Submit(new Order(id: 1, side: Side.Sell, price: 101m, originalQuantity: 5));
        Order orderBuy = new Order(id: 2, side: Side.Buy, price: 99m, originalQuantity: 8);

        List<Trade> trades = orderBookService.Submit(orderBuy);

        Assert.Equal(0, trades.Count);
        Assert.Equal(101m, orderBookService.BestAsk);
        Assert.Equal(99m, orderBookService.BestBid);
        Assert.Equal(2, orderBookService.OrderCount);
    }

    // Submit an order, then Cancel() it. OrderCount should return to 0 and best prices for sell and buy go null
    [Fact]
    public void Cancel_RemovesRestingOrder()
    {
        IMatchingEngineService matchingEngineService = new MatchingEngineService();
        IOrderBookService orderBookService = new OrderBookService(matchingEngineService);

        orderBookService.Submit(new Order(id: 1, side: Side.Buy, price: 101m, originalQuantity: 5));

        Assert.Equal(101m, orderBookService.BestBid);
        Assert.Equal(1, orderBookService.OrderCount);

        bool result = orderBookService.Cancel(1);

        Assert.True(result);
        Assert.Null(orderBookService.BestBid);
        Assert.Equal(0, orderBookService.OrderCount);

        bool secondResult = orderBookService.Cancel(1);
        Assert.False(secondResult);
    }

    // Checks cancel returns false when id is wrong
    [Fact]
    public void Cancel_UnknownId_ReturnsFalse()
    {
        IMatchingEngineService matchingEngineService = new MatchingEngineService();
        IOrderBookService orderBookService = new OrderBookService(matchingEngineService);

        orderBookService.Submit(new Order(id: 1, side: Side.Buy, price: 101m, originalQuantity: 5));

        bool result = orderBookService.Cancel(2);
        Assert.False(result);
    }


    // Checks getDepth method works
    [Fact]
    public void GetDepth_AggregatesAndOrdersLevelsCorrectly()
    {
        IMatchingEngineService matchingEngineService = new MatchingEngineService();
        IOrderBookService orderBookService = new OrderBookService(matchingEngineService);

        orderBookService.Submit(new Order(id: 1, side: Side.Sell, price: 102m, originalQuantity: 5));
        orderBookService.Submit(new Order(id: 2, side: Side.Sell, price: 101m, originalQuantity: 10));
        orderBookService.Submit(new Order(id: 3, side: Side.Sell, price: 101m, originalQuantity: 5));

        orderBookService.Submit(new Order(id: 7, side: Side.Buy, price: 99m, originalQuantity: 5));
        orderBookService.Submit(new Order(id: 9, side: Side.Buy, price: 98m, originalQuantity: 10));

        var askDepth = orderBookService.GetDepth(Side.Sell, 2).ToList();
        var bidDepth = orderBookService.GetDepth(Side.Buy, 2).ToList();

        Assert.Equal(101m, askDepth[0].price);
        Assert.Equal(15, askDepth[0].quantity);
        Assert.Equal(102m, askDepth[1].price);
        Assert.Equal(5, askDepth[1].quantity);

        Assert.Equal(99m, bidDepth[0].price);
        Assert.Equal(98m, bidDepth[1].price);
    }
}
