// TradingSystem.App
//
// This is your harness for exercising and benchmarking the order book as
// you build it. Suggested build order - do each step once the underlying
// OrderBook code supports it, don't try to write all of this at once:
//
// STEP 1 - SMOKE TEST
//   Create an OrderBook, submit a handful of hand-crafted orders (e.g. a
//   resting sell, then a buy that crosses it), and print:
//     - BestBid / BestAsk before and after
//     - any trades returned by Submit()
//   This is just for sanity-checking Submit() while you build it - your
//   real correctness checks belong in TradingSystem.Tests.
//
// STEP 2 - SYNTHETIC MARKET DATA
//   Write a small loop that generates a stream of orders:
//     - maintain a "mid price" that does a random walk (+/- a tick each step)
//     - randomly pick Side.Buy or Side.Sell
//     - pick a price near the mid (some orders should cross the spread,
//       most shouldn't - think about what distribution gives you a
//       realistic mix of resting orders vs trades)
//     - pick a random quantity
//   This becomes your synthetic feed for everything below.
//
// STEP 3 - THROUGHPUT BENCHMARK
//   Feed N orders (start with ~100,000) through the book inside a
//   Stopwatch. Report:
//     - elapsed time and orders/sec
//     - total trades generated
//     - GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2)
//       before vs after - this is your first real look at allocation
//       pressure, and a number worth writing down.
//
// STEP 4 - DEPTH PRINTOUT
//   After the run, print the top 5 bid and ask levels via GetDepth().
//
// Once this runs end-to-end against a correct OrderBook, you'll have a
// baseline "orders/sec" number for the naive SortedDictionary
// implementation. Keep that number - it's what you'll compare a faster v2
// against later.

using TradingSystem.Core;
using TradingSystem.Core.Interfaces;
using TradingSystem.Core.Models;
using TradingSystem.Core.Services;

///// Test 1////
MatchingEngineService matchingEngineService = new MatchingEngineService();
OrderBookService orderBookService = new OrderBookService(matchingEngineService);

var rng = new Random();
decimal mid = 100m;
long nextId = 1;
int totalTrades = 0;

var sw = System.Diagnostics.Stopwatch.StartNew();

int gc0Before = GC.CollectionCount(0);
int gc1Before = GC.CollectionCount(1);
int gc2Before = GC.CollectionCount(2);
for (int i = 0; i < 100000; i++)
{

    mid += rng.Next(0, 2) == 0 ? -1m : 1m;

    Side side = rng.Next(0, 2) == 0 ? Side.Buy : Side.Sell;
    decimal price = mid + rng.Next(-3, 4);
    long quantity = rng.Next(1, 101);

    var order = new Order(id: nextId++, side: side, price: price, originalQuantity: quantity);
    var trades = orderBookService.Submit(order);

    totalTrades += trades.Count;
}
sw.Stop();

double ordersPerSecond = 10000 / sw.Elapsed.TotalSeconds;
Console.WriteLine($"Elapsed: {sw.Elapsed.TotalMilliseconds:F2}ms");
Console.WriteLine($"Orders/sec: {ordersPerSecond:F0}");

Console.WriteLine($"Trades: {totalTrades}");
Console.WriteLine($"BestBid: {orderBookService.BestBid}"); 
Console.WriteLine($"BestAsk: {orderBookService.BestAsk}"); 
Console.WriteLine($"OrderCount: {orderBookService.OrderCount}");
Console.WriteLine("Top 5 Bids:");
foreach (var level in orderBookService.GetDepth(Side.Buy, 5))
    Console.WriteLine($"  Price: {level.price}, Qty: {level.quantity}");

Console.WriteLine("Top 5 Asks:");
foreach (var level in orderBookService.GetDepth(Side.Sell, 5))
    Console.WriteLine($"  Price: {level.price}, Qty: {level.quantity}");


Console.WriteLine($"GC Gen0: {GC.CollectionCount(0) - gc0Before}");
Console.WriteLine($"GC Gen1: {GC.CollectionCount(1) - gc1Before}");
Console.WriteLine($"GC Gen2: {GC.CollectionCount(2) - gc2Before}");
