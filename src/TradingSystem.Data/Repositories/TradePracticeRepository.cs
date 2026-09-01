using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradingSystem.Data.EfCore;
using TradingSystem.Data.Interfaces;

namespace TradingSystem.Data.Repositories
{
    public class TradePracticeRepository : ITradePracticeRepository
    {

        private readonly TradingSystemDbContext _context;

        public TradePracticeRepository(TradingSystemDbContext context)
        {
            _context = context;
        }

        public async Task<List<TradeEntity>> GetTradesBySymbolAsync(string symbol)
        {
            return await _context.Trades.Where(s => s.Symbol == symbol).ToListAsync();
        }

        public async Task<TradeEntity?> GetHighestPriceTradeAsync()
        {
            return await _context.Trades.OrderByDescending(s => s.Price).FirstOrDefaultAsync();
        }

        public async Task<List<TradeEntity>> GetRecentTradesAsync(int count)
        {
            return await _context.Trades.OrderByDescending(s => s.ExecutedAt).Take(count).ToListAsync();
        }

        public async Task<int> GetTradeCountBySymbolAsync(string symbol)
        {
            return await _context.Trades.Where(s => s.Symbol == symbol).CountAsync();
        }

        public async Task AddTradeAsync(TradeEntity trade)
        {
            await _context.Trades.AddAsync(trade);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateTradePriceAsync(int tradeId, decimal newPrice)
        {
            await _context.Trades.Where(t => t.TradeId == tradeId).ExecuteUpdateAsync(s => s.SetProperty(x => x.Price, newPrice));
        }

    }
}
