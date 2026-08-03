# TradingSystem

A practice project for the kind of work that comes up in financial-sector
software engineering roles in London: low-latency systems, high volumes of
data, and being able to talk in detail about *why* something is fast or slow.

## Layout

```
TradingSystem.sln
src/
  TradingSystem.Core/   - domain model + the order book itself (no dependencies)
  TradingSystem.App/    - console harness: demo, synthetic data, benchmarks
tests/
  TradingSystem.Tests/  - correctness tests for the order book
```

`TradingSystem.App` and `TradingSystem.Tests` both reference
`TradingSystem.Core` (already wired up via project references in the
`.csproj` files).

`Models.cs` and `OrderBook.cs` in `TradingSystem.Core` are skeletons -
properties, method signatures, and XML doc comments describing the algorithm
and the design decisions to think through, but `throw new
NotImplementedException()` (or empty) bodies. `OrderBookTests.cs` lists the
correctness tests worth writing, as empty method stubs with comments
describing what each should check.

## Suggested order of work

**1. Models.cs** - `Order` and `Trade`. Small, but forces a few real
decisions (mutability of `Quantity`, validation, value vs reference types).

**2. OrderBook.cs - `Submit()`** - this is the core exercise. Get a single
hand-crafted scenario working first (one resting order, one crossing order)
before worrying about edge cases.

**3. OrderBookTests.cs** - write the tests as you go, not after. The
"same price, older order trades first" test in particular will catch a lot
of subtle bugs in the matching loop.

**4. OrderBook.cs - `Cancel()` and `GetDepth()`** - smaller, but `Cancel()`
is where your choice of data structures for the price levels either pays off
or becomes painful. If it feels painful, that's useful information about the
design, not a sign you did something wrong - revisit the structure.

**5. TradingSystem.App / Program.cs** - smoke test, then synthetic order
generator, then the throughput benchmark with `Stopwatch` and
`GC.CollectionCount`. Write down your baseline orders/sec number once this
runs end-to-end against a correct book.

## Where this goes next

Once you have a correct, benchmarked v1 (SortedDictionary-based book), the
natural next steps - good topics for later sessions, don't try to do these
yet - are:

- **A faster v2 order book**: replace the SortedDictionary-based price
  levels with a flat array of price levels (since prices in practice move
  within a bounded range around the mid) and intrusive linked lists, then
  compare against your v1 baseline with real numbers.
- **BenchmarkDotNet** for proper statistical benchmarking (min/max/percentiles,
  not just a single Stopwatch run) - add via `dotnet add package
  BenchmarkDotNet` once you're on a machine with normal NuGet access.
- **A market data generator + matching engine as separate
  components**, talking over `System.Threading.Channels`, to start thinking
  about producer/consumer pipelines and backpressure.
- **Zero-allocation parsing** with `Span<T>` / `stackalloc` for a simple
  binary or FIX-like message format.
- **Latency percentiles** (p50/p99/p99.9) instead of just throughput, using
  HdrHistogram.

For now: get v1 correct, get it tested, get a baseline number. That's the
whole goal of this iteration.
