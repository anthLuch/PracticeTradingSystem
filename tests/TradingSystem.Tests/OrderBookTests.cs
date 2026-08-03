// TradingSystem.Tests / OrderBookTests
//
// This file lists the correctness tests worth writing for OrderBook.Submit()
// and Cancel(). Method bodies are empty - fill them in as you build the
// OrderBook.
//
// A NOTE ON TEST FRAMEWORK:
// This project currently has no test framework wired up. Once you're ready
// to write real tests, pick one:
//   - xUnit is the de facto standard for .NET: from the project folder,
//       dotnet add package xunit
///      dotnet add package xunit.runner.visualstudio
//       dotnet add package Microsoft.NET.Test.Sdk
//     then mark each method below with [Fact] and run `dotnet test`.
//   - Or write a tiny custom runner (a few lines: iterate over methods via
//     reflection, catch exceptions, print pass/fail) if you'd rather not
//     pull in a dependency yet. Either is fine - the point is the test
//     cases below, not the framework.
//
// using TradingSystem.Core;

namespace TradingSystem.Tests;

public class OrderBookTests
{
    // A buy order with no resting asks should rest in the book untouched,
    // generating zero trades. Check BestBid reflects the new order's price.
    public void NoMatch_WhenBookIsEmpty_OrderRests()
    {
    }

    // A resting sell at price P, then an incoming buy at price P with the
    // same quantity, should produce exactly one trade for the full quantity
    // at price P, and leave both sides of the book empty afterwards.
    public void FullMatch_ExactQuantity_ProducesOneTrade()
    {
    }

    // A resting sell with quantity 10, then an incoming buy with quantity 4
    // at the same price, should produce one trade for 4, and leave a
    // resting sell with quantity 6 (BestAsk unchanged, OrderCount unchanged).
    public void PartialFill_LeavesRemainderResting()
    {
    }

    // Two resting sells at the SAME price, submitted in order (first id A,
    // then id B). An incoming buy that can only fill one of them should
    // match against A (the older one), not B. This is the test that proves
    // your FIFO / time-priority logic, not just your price logic.
    public void SamePrice_OlderOrderTradesFirst()
    {
    }

    // A resting sell at price 100. An incoming buy at price 101 (i.e. willing
    // to pay MORE than the ask) should still trade at 100 - the resting
    // order's price, not the aggressor's limit price.
    public void AggressorGetsRestingPrice_NotOwnLimitPrice()
    {
    }

    // An incoming buy that can't fully fill against the best ask level should
    // continue matching into the NEXT price level if its price still crosses,
    // producing two separate trades at two different prices.
    public void OrderWalksMultiplePriceLevels()
    {
    }

    // A bid at 99 and an ask at 101 (no overlap) should produce zero trades,
    // and both orders should rest in the book - BestBid == 99, BestAsk == 101.
    public void NonCrossingOrders_BothRest_NoTrades()
    {
    }

    // Submit an order, then Cancel() it. OrderCount should return to 0, and
    // BestBid/BestAsk should become null again (assuming it was the only
    // order). A second Cancel() with the same id should return false.
    public void Cancel_RemovesRestingOrder()
    {
    }

    // Cancel() on an id that was never submitted (or already fully filled)
    // should return false, not throw.
    public void Cancel_UnknownId_ReturnsFalse()
    {
    }

    // After a few orders at different prices/sides, GetDepth() should return
    // levels ordered best-price-first, with quantities correctly summed when
    // multiple orders share a price level.
    public void GetDepth_AggregatesAndOrdersLevelsCorrectly()
    {
    }
}
