using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingSystem.Core.Models;

namespace TradingSystem.Data.Interfaces
{
    public interface ITradeRespository
    {
        Task InsertAsync(Trade trade);
    }
}
