using TradingSystem.Core.Models;
using TradingSystem.Data.Models;

namespace TradingSystem.Data.Interfaces
{
    public interface IOrderRepository
    {
        Task InsertAsync(Order order);

        Task UpdateStatusAsync(long orderId, string status);

        Task UpdateAsync(long orderId, long Quantity, bool status);

        Task<IEnumerable<OrderHistoryRecord>> GetAllHistoryAsync(string? symbol, int? limit);
    }
}
