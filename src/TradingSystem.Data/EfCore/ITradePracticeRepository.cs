using System.Collections.Generic;
using System.Threading.Tasks;

namespace TradingSystem.Data.EfCore
{
    public interface ITradePracticeRepository
    {
        Task<List<TradeEntity>> GetTradesBySymbolAsync(string symbol);

        Task<TradeEntity?> GetHighestPriceTradeAsync();

        Task<List<TradeEntity>> GetRecentTradesAsync(int count);

        Task<int> GetTradeCountBySymbolAsync(string symbol);

        Task AddTradeAsync(TradeEntity trade);

        Task UpdateTradePriceAsync(int tradeId, decimal newPrice);
    }
}
