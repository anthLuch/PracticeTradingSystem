using TradingSystem.Core.Models;
namespace TradingSystem.Api.Models
{
    public record OrderPersistenceRequest(Order SubmittedOrder, List<Trade> Trades, List<Order> RestingOrders);

}
