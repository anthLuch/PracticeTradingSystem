using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingSystem.Core.Models;
using TradingSystem.Data.Interfaces;
using TradingSystem.Data.Models;

namespace TradingSystem.Data.Repositories
{
    public class TradeRepository : ITradeRespository
    {
        private readonly string _connectionString;

        public TradeRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("TradingSystemDb")!;
        }


        public async Task InsertAsync(Trade trade)
        {
            const string sql = @"Insert INTO Trades ( TradeId, BuyOrderId, SellOrderId, Symbol, Price, Quantity, ExecutedAt)
                                Values (@TradeId, @BuyOrderId, @SellOrderId, @Symbol, @Price, @Quantity, @ExecutedAt)";


            using IDbConnection connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, trade);

        }

        public async Task<IEnumerable<TradeHistoryRecord>> GetAllHistoryAsync(string? symbol, int? limit)
        {
            int actualLimit = limit ?? 10;

            const string sql = @"Select TradeId, BuyOrderId, SellOrderId, Symbol, Price, Quantity, ExecutedAt
                                From Trades
                                Where(@Symbol IS NULL OR Symbol = @Symbol)
                                ORDER BY ExecutedAt DESC
                                OFFSET 0 ROWS FETCH NEXT @Limit ROWS ONLY";

            using IDbConnection connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<TradeHistoryRecord>(sql, new { Symbol = symbol, Limit = actualLimit });
        }
    }
}
