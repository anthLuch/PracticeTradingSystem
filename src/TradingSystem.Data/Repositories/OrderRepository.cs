using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Dapper;
using TradingSystem.Data.Interfaces;
using TradingSystem.Core.Models;
using System.Data;
using Microsoft.Data.SqlClient;


namespace TradingSystem.Data.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly string _connectionString;

        public OrderRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("TradingSystemDb")!;
        }


        public async Task InsertAsync(Order order)
        {
            const string sql = @"Insert INTO Orders ( OrderId, Symbol, Side, OrderType, Price, OriginalQuantity, Quantity, Status, CreatedAt)
                                Values (@OrderId, @Symbol, @Side, @OrderType, @Price, @OriginalQuantity, @Quantity, @Status, @CreatedAt)";

            var parameters = new
            {
                OrderId = order.Id,
                order.Symbol,
                Side = order.Side.ToString(),
                OrderType = order.OrderType.ToString(),
                order.Price,
                order.OriginalQuantity,
                order.Quantity,
                Status = order.IsFilled ? "Filled" : "Open",
                order.CreatedAt
            };

            using IDbConnection connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, parameters);
        }
                                
        
        public async Task UpdateStatusAsync(long orderId, string status)
        {
            const string sql = @"Update Orders Set Status = @Status
                                Where OrderID = @OrderId";

            var parameters = new
            {
                OrderId = orderId,
                Status = status
            };

            using IDbConnection connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, parameters);

        }
        public async Task UpdateAsync(long orderId, long quantity, bool status)
        {
            const string sql = @"Update Orders Set Quantity = @Quantity, Status = @Status
                                Where OrderID = @OrderId";

            var parameters = new
            {
                OrderId = orderId,
                Quantity = quantity,
                Status = status ? "Filled" : "Open",
            };

            using IDbConnection connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(sql, parameters);

        }
    }
}
