//namespace TradingSystem.Core;

///// <summary>
///// Which side of the book an order sits on / which direction it trades.
///// </summary>
//public enum Side
//{
//    Buy,
//    Sell
//}

///// <summary>
///// A resting or incoming limit order.
/////
///// TODO: implement this class. It needs:
/////   - Id (long) - unique identifier, assigned by whoever creates the order
/////   - Side - Buy or Sell
/////   - Price (decimal) - the limit price
/////   - OriginalQuantity (long) - quantity when first submitted (never changes)
/////   - Quantity (long) - quantity still unfilled; this is the one that
/////     changes as the order gets matched
/////   - Sequence (long) - set by the OrderBook when the order is processed.
/////     Not used for matching logic itself (FIFO order within a price level
/////     should come from the data structure you choose), but useful for
/////     diagnostics/logging and worth having from the start.
/////
///// THINGS TO DECIDE AS YOU IMPLEMENT THIS:
/////   - Should `Quantity` have a public setter? Who is allowed to reduce it,
/////     and how do you stop it going negative?
/////   - What should the constructor validate? What's a sensible exception to
/////     throw for a zero or negative price/quantity?
/////   - Add an `IsFilled` convenience property (Quantity == 0).
///// </summary>
//public sealed class Order
//{
//    // TODO: properties

//    // TODO: constructor(s)

//    // TODO: a method (internal, or however you design it) for the book to
//    // reduce remaining quantity when a match occurs
//}

///// <summary>
///// A single execution between two orders - an immutable record of something
///// that happened. Once created, a Trade should never change.
/////
///// TODO: implement this with:
/////   - TradeId (long)
/////   - BuyOrderId (long)
/////   - SellOrderId (long)
/////   - Price (decimal) - the price the trade executed at
/////   - Quantity (long) - the quantity that traded
/////
///// THINGS TO THINK ABOUT:
/////   - `record struct` vs `record class` vs plain `class` - you'll be
/////     creating one of these per match, potentially hundreds of thousands
/////     per benchmark run. What are the tradeoffs? (This is a good thing to
/////     come back to once you have a benchmark running and can measure it
/////     rather than guess.)
/////   - `readonly record struct` gives you value semantics and immutability
/////     for free via the positional record syntax - worth knowing the syntax
/////     even if you decide against it here.
///// </summary>
//public readonly record struct Trade
//{
//    // TODO: define the fields/properties listed above
//}
