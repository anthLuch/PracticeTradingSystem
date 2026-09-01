using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingSystem.Data.EfCore;
using TradingSystem.Data.Interfaces;
using TradingSystem.Data.Models;

namespace TradingSystem.Data.Repositories
{
    public class TradeSummaryRepository : ITradeSummaryRepository
    {
        private readonly TradingSystemDbContext _context;

        public TradeSummaryRepository(TradingSystemDbContext context)
        {
            _context = context;
        }

        public async Task<List<TradeSummaryRecord>> GetAllTradesAsync()
        {
            return await _context.Trades.GroupBy(t => t.Symbol)
                                        .Select(s => new TradeSummaryRecord
                                        {
                                            Symbol = s.Key,
                                            TotalQuantity = s.Sum(x => x.Quantity),
                                            AveragePrice = s.Average(x => x.Price),
                                            TradeCount = s.Count()
                                        }).ToListAsync();

        }
    }
}
