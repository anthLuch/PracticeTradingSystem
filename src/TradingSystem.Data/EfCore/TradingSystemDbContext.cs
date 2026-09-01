using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TradingSystem.Data.EfCore
{
    public class TradingSystemDbContext : DbContext
    {
        public TradingSystemDbContext(DbContextOptions<TradingSystemDbContext> options) : base(options) { }

        public DbSet<TradeEntity> Trades => Set<TradeEntity>();
    }
}
