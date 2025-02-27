using FixMessageAnalyzer.Data;
using FixMessageAnalyzer.Data.DTOs;
using FixMessageAnalyzer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using System.Linq;
using System.Text.Json;

namespace FixMessageAnalyzer.Services
{
    public interface IFixOrderService
    {
        Task<OrderFlowResponse> GetOrderFlowAsync(OrderFlowFilterDto filter);
    }
}

namespace FixMessageAnalyzer.Services
{
    public class FixOrderService : IFixOrderService
    {
        private readonly FixDbContext _dbContext;

        public FixOrderService(FixDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<OrderFlowResponse> GetOrderFlowAsync(OrderFlowFilterDto filter)
        {
            var pageSize = filter.PageSize ?? 10;
            var pageNumber = filter.PageNumber ?? 1;
            var skip = (pageNumber - 1) * pageSize;

            // Define parameters with explicit types
            var parameters = new[]
            {
                new Npgsql.NpgsqlParameter("@OrderId", NpgsqlTypes.NpgsqlDbType.Text)
                    { Value = (object)filter.OrderId ?? DBNull.Value },
                new Npgsql.NpgsqlParameter("@ClOrdId", NpgsqlTypes.NpgsqlDbType.Text)
                    { Value = (object)filter.ClOrdId ?? DBNull.Value },
                new Npgsql.NpgsqlParameter("@Symbol", NpgsqlTypes.NpgsqlDbType.Text)
                    { Value = (object)filter.Symbol ?? DBNull.Value },
                new Npgsql.NpgsqlParameter("@StartDate", NpgsqlTypes.NpgsqlDbType.Timestamp)
                    { Value = (object)filter.StartDate ?? DBNull.Value },
                new Npgsql.NpgsqlParameter("@EndDate", NpgsqlTypes.NpgsqlDbType.Timestamp)
                    { Value = (object)filter.EndDate ?? DBNull.Value },
                new Npgsql.NpgsqlParameter("@Skip", NpgsqlTypes.NpgsqlDbType.Integer)
                    { Value = skip },
                new Npgsql.NpgsqlParameter("@Take", NpgsqlTypes.NpgsqlDbType.Integer)
                    { Value = pageSize }
            };

            // Updated SQL to handle both OrderID and ClOrdID
            var sql = @"
                SELECT *
                FROM fix.messages
                WHERE msg_type IN ('D', '8', '9')
                  AND (
                    (@OrderId IS NULL OR ""Fields""->>'37' = @OrderId)
                    OR (@ClOrdId IS NULL OR ""Fields""->>'11' = @ClOrdId OR ""Fields""->>'41' = @ClOrdId)
                  )
                  AND (@Symbol IS NULL OR ""Fields""->>'55' = @Symbol)
                  AND (@StartDate IS NULL OR timestamp >= @StartDate)
                  AND (@EndDate IS NULL OR timestamp <= @EndDate)
                ORDER BY timestamp DESC
                OFFSET @Skip
                LIMIT @Take";

            var messages = await _dbContext.Messages
                .FromSqlRaw(sql, parameters)
                .ToListAsync();

            var countSql = @"
                SELECT COUNT(*)
                FROM fix.messages
                WHERE msg_type IN ('D', '8', '9')
                  AND (
                    (@OrderId IS NULL OR ""Fields""->>'37' = @OrderId)
                    OR (@ClOrdId IS NULL OR ""Fields""->>'11' = @ClOrdId OR ""Fields""->>'41' = @ClOrdId)
                  )
                  AND (@Symbol IS NULL OR ""Fields""->>'55' = @Symbol)
                  AND (@StartDate IS NULL OR timestamp >= @StartDate)
                  AND (@EndDate IS NULL OR timestamp <= @EndDate)";

            int totalCount = 0;
            using (var conn = _dbContext.Database.GetDbConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = countSql;
                    foreach (var param in parameters.Where(p => p.ParameterName != "@Skip" && p.ParameterName != "@Take"))
                    {
                        var clone = cmd.CreateParameter();
                        clone.ParameterName = param.ParameterName;
                        clone.DbType = param.DbType;
                        clone.Value = param.Value;
                        cmd.Parameters.Add(clone);
                    }
                    totalCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
            }

            var orders = ProcessMessages(messages, filter.TrackingMode ?? OrderTrackingMode.ClOrdId);

            return new OrderFlowResponse
            {
                Orders = orders,
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = pageNumber
            };
        }

        private List<OrderFlowDto> ProcessMessages(List<FixMessage> messages, OrderTrackingMode trackingMode)
        {
            var orders = new Dictionary<string, OrderFlowDto>();
            var orderLinks = new Dictionary<string, string>(); // Track ClOrdID to OrderID relationships

            foreach (var message in messages.OrderBy(m => m.Timestamp))
            {
                var json = JsonSerializer.Serialize(message.Fields);
                using var doc = JsonDocument.Parse(json);
                var fields = doc.RootElement;

                string trackingId = GetTrackingId(fields, message.MsgType, trackingMode);
                if (string.IsNullOrEmpty(trackingId))
                    continue;

                // Handle order modifications
                string originalId = GetOriginalOrderId(fields, trackingMode);
                if (!string.IsNullOrEmpty(originalId) && orders.ContainsKey(originalId))
                {
                    trackingId = originalId;
                }

                // Create or update order
                if (!orders.ContainsKey(trackingId))
                {
                    var order = CreateOrderFromMessage(message, fields);
                    if (order != null)
                    {
                        orders[trackingId] = order;

                        // Store relationship between ClOrdID and OrderID
                        if (fields.TryGetProperty("11", out var clOrdIdElement) &&
                            fields.TryGetProperty("37", out var orderIdElement))
                        {
                            orderLinks[clOrdIdElement.GetString()] = orderIdElement.GetString();
                        }
                    }
                }

                // Add state to order
                if (orders.ContainsKey(trackingId))
                {
                    var state = CreateStateFromMessage(message, fields);
                    if (state != null)
                    {
                        orders[trackingId].States.Add(state);
                    }
                }
            }

            // Sort states by timestamp for each order
            foreach (var order in orders.Values)
            {
                order.States = order.States.OrderBy(s => s.Timestamp).ToList();
            }

            return orders.Values
                .OrderByDescending(o => o.CreatedAt)
                .ToList();
        }

        private string GetTrackingId(JsonElement fields, string msgType, OrderTrackingMode trackingMode)
        {
            switch (trackingMode)
            {
                case OrderTrackingMode.OrderId:
                    return fields.TryGetProperty("37", out var orderIdElement) ? orderIdElement.GetString() : null;

                case OrderTrackingMode.ClOrdId:
                    if (fields.TryGetProperty("11", out var clOrdIdElement))
                        return clOrdIdElement.GetString();
                    // For executions, we might only have OrderID
                    if (msgType == "8" && fields.TryGetProperty("37", out var execOrderIdElement))
                        return execOrderIdElement.GetString();
                    return null;

                default:
                    throw new ArgumentException("Invalid tracking mode");
            }
        }

        private string GetOriginalOrderId(JsonElement fields, OrderTrackingMode trackingMode)
        {
            // Check OrigClOrdID for modifications
            if (trackingMode == OrderTrackingMode.ClOrdId &&
                fields.TryGetProperty("41", out var origClOrdIdElement))
            {
                return origClOrdIdElement.GetString();
            }
            return null;
        }

        private OrderFlowDto CreateOrderFromMessage(FixMessage message, JsonElement fields)
        {
            try
            {
                if (!fields.TryGetProperty("37", out var ordIdElement) ||
                    !fields.TryGetProperty("55", out var symbolElement) ||
                    !fields.TryGetProperty("54", out var sideElement) ||
                    !fields.TryGetProperty("38", out var qtyElement) ||
                    !fields.TryGetProperty("44", out var priceElement))
                {
                    return null;
                }

                return new OrderFlowDto
                {
                    OrderId = ordIdElement.GetString(),
                    Symbol = symbolElement.GetString(),
                    Side = GetSide(sideElement.GetString()),
                    Quantity = decimal.Parse(qtyElement.GetString()),
                    Price = decimal.Parse(priceElement.GetString()),
                    CreatedAt = message.Timestamp,
                    States = new List<OrderStateDto>()
                };
            }
            catch
            {
                return null;
            }
        }

        private OrderStateDto CreateStateFromMessage(FixMessage message, JsonElement fields)
        {
            return new OrderStateDto
            {
                Status = GetStatus(message.MsgType, fields),
                Timestamp = message.Timestamp,
                Details = GetDetails(message.MsgType, fields)
            };
        }

        private string GetStatus(string msgType, JsonElement fields)
        {
            switch (msgType)
            {
                case "D":
                    return "New";
                case "8":
                    if (fields.TryGetProperty("39", out var ordStatusElement))
                    {
                        return ordStatusElement.GetString() switch
                        {
                            "0" => "New",
                            "4" => "Cancelled",
                            "8" => "Rejected",
                            "1" => "Partial",
                            "2" => "Filled",
                            _ => "Unknown"
                        };
                    }
                    return "Unknown";
                case "9":
                    return "Rejected";
                default:
                    return "Unknown";
            }
        }

        private string GetDetails(string msgType, JsonElement fields)
        {
            if (msgType == "8")
            {
                if (fields.TryGetProperty("39", out var ordStatusElement))
                {
                    var execType = ordStatusElement.GetString();
                    if (execType == "1" || execType == "2") // Partial or Filled
                    {
                        if (fields.TryGetProperty("32", out var lastQtyElement) &&
                            fields.TryGetProperty("31", out var lastPxElement))
                        {
                            return $"{lastQtyElement.GetString()}@{lastPxElement.GetString()}";
                        }
                    }
                    else if (execType == "8" && fields.TryGetProperty("58", out var textElement))
                    {
                        return textElement.GetString(); // Rejection text
                    }
                }
            }
            else if (msgType == "9" && fields.TryGetProperty("58", out var textElement))
            {
                return textElement.GetString(); // Cancel reject reason
            }

            return null;
        }

        private string GetSide(string side)
        {
            return side switch
            {
                "1" => "Buy",
                "2" => "Sell",
                _ => side
            };
        }
    }
}