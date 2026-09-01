using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingSystem.Data.EfCore;
using TradingSystem.Data.Models;

namespace TradingSystem.Data.Interfaces
{
    public interface ITradeSummaryRepository
    {
        Task<List<TradeSummaryRecord>> GetAllTradesAsync();
    }
}
