using System.Diagnostics;
using TradingSystem.Core;
using TradingSystem.Core.Interfaces;
using TradingSystem.Core.Models;
using TradingSystem.Core.Services;

const int Orders = 100_000;
const int Seed = 42;

// --- V1 Benchmark ---
var matchingEngineV1 = new MatchingEngineService();
var orderBookV1 = new OrderBookService(matchingEngineV1);
PositionTracker tracker = new PositionTracker();

var rng = new Random(Seed);
decimal mid = 100m;
long nextId = 1;
int totalTradesV1 = 0;

int gc0Before = GC.CollectionCount(0);
int gc1Before = GC.CollectionCount(1);
int gc2Before = GC.CollectionCount(2);

var sw = System.Diagnostics.Stopwatch.StartNew();

for (int i = 0; i < Orders; i++)
{
    mid += rng.Next(0, 2) == 0 ? -1m : 1m;
    Side side = rng.Next(0, 2) == 0 ? Side.Buy : Side.Sell;
    decimal price = mid + rng.Next(-3, 4);
    long quantity = rng.Next(1, 101);

    var order = new Order(id: nextId++, side: side, orderType: OrderType.Limit, symbol: "AAPL", price: price, originalQuantity: quantity, createdAt: DateTime.UtcNow);
    List<Trade> trades = orderBookV1.Submit(order);

    foreach (Trade trade in trades)
    {
        tracker.ProcessTrade(trade, order.Side);
    }
    totalTradesV1 += trades.Count;
}

sw.Stop();

Console.WriteLine("--- V1 Results ---");
Console.WriteLine($"Elapsed: {sw.Elapsed.TotalMilliseconds:F2}ms");
Console.WriteLine($"Orders/sec: {Orders / sw.Elapsed.TotalSeconds:F0}");
Console.WriteLine($"Total Trades: {totalTradesV1}");
Console.WriteLine($"BestBid: {orderBookV1.BestBid}");
Console.WriteLine($"BestAsk: {orderBookV1.BestAsk}");
Console.WriteLine($"OrderCount: {orderBookV1.OrderCount}");
Console.WriteLine($"Position: {tracker.Position}");
Console.WriteLine($"AverageEntryPrice: {tracker.AverageEntryPrice}");
Console.WriteLine($"Realised PL: {tracker.RealisedPL}");
Console.WriteLine($"Unrealised PL: {tracker.UnrealisedPL}");
Console.WriteLine("Top 5 Bids:");
foreach (var level in orderBookV1.GetDepth(Side.Buy, 5))
    Console.WriteLine($"  Price: {level.price}, Qty: {level.quantity}");
Console.WriteLine("Top 5 Asks:");
foreach (var level in orderBookV1.GetDepth(Side.Sell, 5))
    Console.WriteLine($"  Price: {level.price}, Qty: {level.quantity}");
Console.WriteLine($"GC Gen0: {GC.CollectionCount(0) - gc0Before}");
Console.WriteLine($"GC Gen1: {GC.CollectionCount(1) - gc1Before}");
Console.WriteLine($"GC Gen2: {GC.CollectionCount(2) - gc2Before}");

//await Task.Run(async () =>
//{
    // --- V2 Benchmark ---
IMatchingEngineServiceV2 matchingEngineV2 = new MatchingEngineServiceV2(0.1m);
IOrderBookService orderBookV2 = new OrderBookServiceV2(matchingEngineV2, 0.1m);
OrderBookProcessing processor = new OrderBookProcessing(orderBookV2);
PositionTracker trackerV2 = new PositionTracker();

var cts = new CancellationTokenSource();
_ = Task.Run(() => processor.StartAsync(cts.Token));

rng = new Random(Seed);
mid = 100m;
nextId = 1;
int totalTradesV2 = 0;

int gc0BeforeV2 = GC.CollectionCount(0);
int gc1BeforeV2 = GC.CollectionCount(1);
int gc2BeforeV2 = GC.CollectionCount(2);

var sw2 = System.Diagnostics.Stopwatch.StartNew();

for (int i = 0; i < Orders; i++)
{
    mid += rng.Next(0, 2) == 0 ? -1m : 1m;
    Side side = rng.Next(0, 2) == 0 ? Side.Buy : Side.Sell;
    decimal price = mid + rng.Next(-3, 4);
    long quantity = rng.Next(1, 101);

    var order = new Order(id: nextId++, side: side, orderType: OrderType.Limit, symbol: "AAPL", price: price, originalQuantity: quantity, createdAt: DateTime.UtcNow);
    List<Trade> trades = orderBookV2.Submit(order);
    foreach(Trade trade in trades)
    {
        trackerV2.ProcessTrade(trade, order.Side);
    }
    //List<Trade> trades = await processor.SubmitAsync(order);
    totalTradesV2 += trades.Count;
}

sw2.Stop();

Console.WriteLine("\n--- V2 Results ---");
Console.WriteLine($"Elapsed: {sw2.Elapsed.TotalMilliseconds:F2}ms");
Console.WriteLine($"Orders/sec: {Orders / sw2.Elapsed.TotalSeconds:F0}");
Console.WriteLine($"Total Trades: {totalTradesV2}");
Console.WriteLine($"BestBid: {orderBookV2.BestBid}");
Console.WriteLine($"BestAsk: {orderBookV2.BestAsk}");
Console.WriteLine($"OrderCount: {orderBookV2.OrderCount}");
Console.WriteLine($"Position: {trackerV2.Position}");
Console.WriteLine($"AverageEntryPrice: {trackerV2.AverageEntryPrice}");
Console.WriteLine($"Realised PL: {trackerV2.RealisedPL}");
Console.WriteLine($"Unrealised PL: {trackerV2.UnrealisedPL}");

Console.WriteLine("Top 5 Bids:");
foreach (var level in orderBookV2.GetDepth(Side.Buy, 5))
    Console.WriteLine($"  Price: {level.price}, Qty: {level.quantity}");
Console.WriteLine("Top 5 Asks:");
foreach (var level in orderBookV2.GetDepth(Side.Sell, 5))
    Console.WriteLine($"  Price: {level.price}, Qty: {level.quantity}");
Console.WriteLine($"GC Gen0: {GC.CollectionCount(0) - gc0BeforeV2}");
Console.WriteLine($"GC Gen1: {GC.CollectionCount(1) - gc1BeforeV2}");
Console.WriteLine($"GC Gen2: {GC.CollectionCount(2) - gc2BeforeV2}");
//});

