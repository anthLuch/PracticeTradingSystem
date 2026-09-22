# TradingSystem

A limit order book and matching engine in C# / .NET (net8.0 core, net9.0 API), with an ASP.NET Core REST API,
SQL Server persistence and a React + TypeScript front end.

The focus is the engine: correct price-time matching, and two order book implementations
benchmarked against each other to show *why* one is faster than the other.

<!-- Add a screenshot of the UI here: ![Order book UI](docs/screenshot.png) -->

## Features

- **Order types:** Limit, Market and Fill-or-Kill
- **Price-time priority:** best price first, then oldest order first at the same price
- **Cancel** in O(1) via an order-id → linked-list-node index
- **Market depth:** aggregated quantity per price level, top N levels
- **Position tracking:** net position, average entry price, realised and unrealised P&L
- **REST API** for submitting and cancelling orders, depth, position and history
- **Persistence** of orders and trades to SQL Server, off the matching path
- **React UI** showing the live book, an order entry form, and order/trade history

## Architecture

```
React UI  ──HTTP──▶  ASP.NET Core API
                          │
                          ▼
                  Channel<OrderRequest>          (many API requests → one consumer)
                          │
                          ▼
                  Matching engine + order book   (single-threaded, no locks)
                          │
                          ▼
            Channel<OrderPersistenceRequest>     (fire-and-forget to the DB writer)
                          │
                          ▼
                      SQL Server
```

## Design decisions

### v1 order book – `SortedDictionary`
Each side of the book is a `SortedDictionary<decimal, LinkedList<Order>>`: price levels
kept in sorted order, with a FIFO queue of orders at each level. Bids use a reversed
comparer so the best price is always first. Simple and correct, but every insert and
level removal is O(log n) with tree-node allocations.

### v2 order book – flat arrays
Prices are converted to integer ticks (`price / tickSize`) and used as an index into a
flat array of price levels, with the best bid and best ask tracked explicitly.

- Finding, adding to or removing a level is O(1) array indexing instead of a tree lookup.
- When the best level empties, the engine scans to the next non-empty level. Prices
  cluster near the top of the book, so this scan is usually short.
- **Trade-off:** memory. The array covers the full price range up front. At a 0.01 tick
  over 0–100,000 that is ~10 million slots per side (~160 MB of references in total).
  A real system would use a window around the mid price instead.

### Single-writer matching
The API receives requests concurrently but pushes them into a `System.Threading.Channels`
channel with a single consumer. Only one thread ever touches the book, so it needs no
locks, and ordering is deterministic. Each request carries a `TaskCompletionSource` so the
API can await its trades.

### Persistence off the hot path
Database writes go through a second channel to a background writer, so a slow insert never
blocks matching. The cost is that the database is eventually consistent with the in-memory
book. Order and trade inserts use Dapper; the trade summary query uses EF Core.

## Benchmarks

`TradingSystem.App` replays the same 100,000 randomly generated limit orders (fixed seed)
through both books, measuring throughput with `Stopwatch` and GC counts with
`GC.CollectionCount`.

| Book | Orders/sec | Gen0 GCs | Gen1 GCs | Gen2 GCs |
|------|-----------:|---------:|---------:|---------:|
| v1 (`SortedDictionary`) | 365.31ms, 273,744 orders/Sec | 13 | 4 | 3 |
| v2 (flat array)         | 139.70ms, 715,828 orders/Sec | 4 | 2 | 0 |

## Project layout

```
TradingSystem.sln
src/
  TradingSystem.Core/   Domain model, order books (v1, v2), matching engines, position tracker
  TradingSystem.Api/    ASP.NET Core API, controllers, background DB writer
  TradingSystem.Data/   Repositories (Dapper + EF Core)
  TradingSystem.App/    Console benchmark: v1 vs v2
tests/
  TradingSystem.Tests/  xUnit tests for matching, cancel, depth and edge cases
db/
  schema.sql            Orders and Trades tables
  seed.sql              Sample data
orderbook-ui/           React + TypeScript + Vite front end
```

## Running it

**Prerequisites:** .NET 9 SDK, Node.js, SQL Server (LocalDB or full)

```bash
# Tests
dotnet test

# Benchmark
dotnet run -c Release --project src/TradingSystem.App

# Database: run db/schema.sql (and optionally db/seed.sql) against a database
# called TradingSystemDb, or edit the connection string in
# src/TradingSystem.Api/appsettings.json

# API (http://localhost:5133)
dotnet run --project src/TradingSystem.Api

# UI (http://localhost:5173)
cd orderbook-ui
npm install
npm run dev
```

### API endpoints

| Method | Route | Description |
|--------|-------|-------------|
| POST   | `/api/order`                          | Submit an order, returns resulting trades |
| DELETE | `/api/order/{orderId}`                | Cancel a resting order |
| GET    | `/api/order/depth?side=0&levels=5`    | Top N price levels (0 = bids, 1 = asks) |
| GET    | `/api/order/position`                 | Current position and P&L |
| GET    | `/api/order/history?symbol=&limit=`   | Order history |
| GET    | `/api/trade/history?symbol=`          | Trade history |
| GET    | `/api/trade/summary`                  | Aggregated trade summary |


## Next steps

- BenchmarkDotNet for statistically sound benchmarks
- Latency percentiles (p50 / p99 / p99.9) with HdrHistogram, not just throughput
- Zero-allocation message parsing with `Span<T>` for a FIX-like binary format
- Multiple symbols with one book per symbol
- Bounded channels with backpressure
