using FixMessageAnalyzer.Data;
using FixMessageAnalyzer.Data.DTOs;
using FixMessageAnalyzer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using QuickFix.Fields;
using System.Data.Common;
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
            // Set default values: 5 latest orders when no parameters are provided
            var hasAnyFilter = filter.OrderId != null ||
                               filter.ClOrdId != null ||
                               filter.Symbol != null ||
                               filter.StartDate.HasValue ||
                               filter.EndDate.HasValue;

            var pageSize = filter.PageSize ?? (hasAnyFilter ? 10 : 5);
            var pageNumber = filter.PageNumber ?? 1;
            var skip = (pageNumber - 1) * pageSize;

            try
            {
                // Ensure StartDate and EndDate are UTC
                if (filter.StartDate.HasValue && filter.StartDate.Value.Kind != DateTimeKind.Utc)
                {
                    filter.StartDate = DateTime.SpecifyKind(filter.StartDate.Value, DateTimeKind.Utc);
                }

                if (filter.EndDate.HasValue && filter.EndDate.Value.Kind != DateTimeKind.Utc)
                {
                    filter.EndDate = DateTime.SpecifyKind(filter.EndDate.Value, DateTimeKind.Utc);
                }

                int totalCount;
                List<FixMessage> messages;

                // If no filters, use simple query for latest orders
                if (!hasAnyFilter)
                {
                    var simpleQuery = _dbContext.Messages
                        .Where(m => new[] { "D", "8", "9" }.Contains(m.MsgType))
                        .OrderByDescending(m => m.Timestamp);

                    totalCount = await simpleQuery.CountAsync();

                    messages = await simpleQuery
                        .Skip(skip)
                        .Take(pageSize)
                        .ToListAsync();
                }
                else
                {
                    // For JSONB fields, use direct SQL
                    var sql = @"
                    SELECT id 
                    FROM fix.messages 
                    WHERE msg_type IN ('D', '8', '9')
                    AND (@OrderId IS NULL OR ""Fields""->>'37' = @OrderId)
                    AND (@ClOrdId IS NULL OR ""Fields""->>'11' = @ClOrdId OR ""Fields""->>'41' = @ClOrdId)
                    AND (@Symbol IS NULL OR ""Fields""->>'55' = @Symbol)
                    AND (@StartDate IS NULL OR timestamp >= @StartDate)
                    AND (@EndDate IS NULL OR timestamp <= @EndDate)
                    ORDER BY timestamp DESC";

                    // Add count query
                    var countSql = @"
                    SELECT COUNT(*)
                    FROM fix.messages 
                    WHERE msg_type IN ('D', '8', '9')
                    AND (@OrderId IS NULL OR ""Fields""->>'37' = @OrderId)
                    AND (@ClOrdId IS NULL OR ""Fields""->>'11' = @ClOrdId OR ""Fields""->>'41' = @ClOrdId)
                    AND (@Symbol IS NULL OR ""Fields""->>'55' = @Symbol)
                    AND (@StartDate IS NULL OR timestamp >= @StartDate)
                    AND (@EndDate IS NULL OR timestamp <= @EndDate)";

                    List<int> matchingIds = new List<int>();

                    // Important: Create a new scope for the connection to ensure it's properly disposed
                    var connectionString = _dbContext.Database.GetDbConnection().ConnectionString;
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = countSql;
                            AddParameters(command, filter, null, null);
                            totalCount = Convert.ToInt32(await command.ExecuteScalarAsync());
                        }

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = sql + " OFFSET @Skip LIMIT @Take";
                            AddParameters(command, filter, skip, pageSize);

                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    matchingIds.Add(reader.GetInt32(0));
                                }
                            }
                        }
                    }

                    // Now load the full entities by ID
                    messages = matchingIds.Count > 0
                    ? await _dbContext.Messages
                        .AsNoTracking() // Prevents tracking, improving performance
                        .Where(m => matchingIds.Contains(m.Id))
                        .OrderByDescending(m => m.Timestamp)
                        .ToListAsync()
                    : new List<FixMessage>();
                }

                // Process messages into order flow structure
                var orders = ProcessMessages(messages, filter.TrackingMode ?? OrderTrackingMode.ClOrdId);

                return new OrderFlowResponse
                {
                    Orders = orders,
                    TotalCount = totalCount,
                    PageSize = pageSize,
                    CurrentPage = pageNumber
                };
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error in GetOrderFlowAsync");
                throw;
            }
        }

        // Helper to add parameters to both SQL commands
        private void AddParameters(DbCommand command, OrderFlowFilterDto filter, int? skip, int? take)
        {
            // Use NpgsqlParameter to explicitly set types
            var orderIdParam = command.CreateParameter();
            orderIdParam.ParameterName = "@OrderId";
            orderIdParam.Value = filter.OrderId ?? (object)DBNull.Value;
            if (filter.OrderId == null)
            {
                ((NpgsqlParameter)orderIdParam).NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Text;
            }
            command.Parameters.Add(orderIdParam);

            var clOrdIdParam = command.CreateParameter();
            clOrdIdParam.ParameterName = "@ClOrdId";
            clOrdIdParam.Value = filter.ClOrdId ?? (object)DBNull.Value;
            if (filter.ClOrdId == null)
            {
                ((NpgsqlParameter)clOrdIdParam).NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Text;
            }
            command.Parameters.Add(clOrdIdParam);

            var symbolParam = command.CreateParameter();
            symbolParam.ParameterName = "@Symbol";
            symbolParam.Value = filter.Symbol ?? (object)DBNull.Value;
            if (filter.Symbol == null)
            {
                ((NpgsqlParameter)symbolParam).NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Text;
            }
            command.Parameters.Add(symbolParam);

            if (filter.StartDate.HasValue)
            {
                var startDateParam = command.CreateParameter();
                startDateParam.ParameterName = "@StartDate";
                startDateParam.Value = filter.StartDate.Value;
                ((NpgsqlParameter)startDateParam).NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.TimestampTz;
                command.Parameters.Add(startDateParam);
            }
            else
            {
                var startDateParam = command.CreateParameter();
                startDateParam.ParameterName = "@StartDate";
                startDateParam.Value = DBNull.Value;
                ((NpgsqlParameter)startDateParam).NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.TimestampTz;
                command.Parameters.Add(startDateParam);
            }

            if (filter.EndDate.HasValue)
            {
                var endDateParam = command.CreateParameter();
                endDateParam.ParameterName = "@EndDate";
                endDateParam.Value = filter.EndDate.Value;
                ((NpgsqlParameter)endDateParam).NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.TimestampTz;
                command.Parameters.Add(endDateParam);
            }
            else
            {
                var endDateParam = command.CreateParameter();
                endDateParam.ParameterName = "@EndDate";
                endDateParam.Value = DBNull.Value;
                ((NpgsqlParameter)endDateParam).NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.TimestampTz;
                command.Parameters.Add(endDateParam);
            }

            // Add pagination parameters if provided
            if (skip.HasValue)
            {
                var skipParam = command.CreateParameter();
                skipParam.ParameterName = "@Skip";
                skipParam.Value = skip.Value;
                ((NpgsqlParameter)skipParam).NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Integer;
                command.Parameters.Add(skipParam);
            }

            if (take.HasValue)
            {
                var takeParam = command.CreateParameter();
                takeParam.ParameterName = "@Take";
                takeParam.Value = take.Value;
                ((NpgsqlParameter)takeParam).NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Integer;
                command.Parameters.Add(takeParam);
            }
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