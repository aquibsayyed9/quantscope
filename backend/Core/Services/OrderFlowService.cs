using FixMessageAnalyzer.Data;
using FixMessageAnalyzer.Data.DTOs;
using FixMessageAnalyzer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FixMessageAnalyzer.Services
{
    public interface IOrderFlowService
    {
        Task<OrderFlowResponse> GetOrderFlowAsync(OrderFlowFilterDto filter);
    }

    public class OrderFlowService : IOrderFlowService
    {
        private readonly FixDbContext _context;

        public OrderFlowService(FixDbContext context)
        {
            _context = context;
        }

        public async Task<OrderFlowResponse> GetOrderFlowAsync(OrderFlowFilterDto filter)
        {
            var query = _context.Messages
                .AsNoTracking()
                .Where(m => m.MsgType.StartsWith("D") ||  // New Order Single
                           m.MsgType == "8" ||        // Execution Report
                           m.MsgType == "9");             // Order Cancel Reject

            // Apply filters using the dictionary keys directly
            if (!string.IsNullOrEmpty(filter.OrderId))
            {
                query = query.Where(m => m.Fields.ContainsKey("11") && m.Fields["11"] == filter.OrderId);
            }

            if (!string.IsNullOrEmpty(filter.Symbol))
            {
                query = query.Where(m => m.Fields.ContainsKey("55") && m.Fields["55"] == filter.Symbol);
            }

            if (filter.StartDate.HasValue)
            {
                query = query.Where(m => m.Timestamp >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(m => m.Timestamp <= filter.EndDate.Value);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var pageSize = filter.PageSize ?? 10;
            var pageNumber = filter.PageNumber ?? 1;

            var messages = await query
                .OrderByDescending(m => m.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Group messages by ClOrdID (key "11") to create order flows
            var orderFlows = new List<OrderFlowDto>();
            var messageGroups = messages
                .Select(m => new { Message = m, Fields = m.Fields })
                .Where(x => x.Fields.ContainsKey("11"))
                .GroupBy(x => x.Fields["11"])
                .ToList();

            foreach (var group in messageGroups)
            {
                var firstMessage = group.OrderBy(x => x.Message.Timestamp).First();
                var orderFlow = CreateOrderFlow(firstMessage.Message);

                if (orderFlow != null)
                {
                    foreach (var item in group.OrderBy(x => x.Message.Timestamp))
                    {
                        var state = CreateOrderState(item.Message);
                        if (state != null)
                        {
                            orderFlow.States.Add(state);
                        }
                    }
                    orderFlows.Add(orderFlow);
                }
            }

            return new OrderFlowResponse
            {
                Orders = orderFlows.OrderByDescending(o => o.CreatedAt).ToList(),
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = pageNumber
            };
        }

        private OrderFlowDto CreateOrderFlow(FixMessage message)
        {
            try
            {
                var json = JsonSerializer.Serialize(message.Fields);
                using var doc = JsonDocument.Parse(json);
                var fields = doc.RootElement;

                if (!fields.TryGetProperty("11", out var clOrdIdElement) ||
                    !fields.TryGetProperty("55", out var symbolElement) ||
                    !fields.TryGetProperty("54", out var sideElement) ||
                    !fields.TryGetProperty("38", out var qtyElement) ||
                    !fields.TryGetProperty("44", out var priceElement))
                {
                    return null;
                }

                return new OrderFlowDto
                {
                    OrderId = clOrdIdElement.GetString(),
                    Symbol = symbolElement.GetString(),
                    Side = GetSide(sideElement.GetString()),
                    Quantity = decimal.Parse(qtyElement.GetString()),
                    Price = decimal.Parse(priceElement.GetString()),
                    CreatedAt = message.Timestamp,
                    States = new List<OrderStateDto>()
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        private OrderStateDto CreateOrderState(FixMessage message)
        {
            try
            {
                var json = JsonSerializer.Serialize(message.Fields);
                using var doc = JsonDocument.Parse(json);
                var fields = doc.RootElement;
                
                return new OrderStateDto
                {
                    Status = GetOrderStatus(message.MsgType, fields),
                    Timestamp = message.Timestamp,
                    Details = GetStateDetails(message.MsgType, fields)
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string GetOrderStatus(string msgType, JsonElement fields)
        {
            switch (msgType)
            {
                case "D":
                    return "New";
                case "8":
                    if (fields.TryGetProperty("150", out var execTypeElement))
                    {
                        var execType = execTypeElement.GetString();
                        switch (execType)
                        {
                            case "0": return "New";
                            case "4": return "Cancelled";
                            case "8": return "Rejected";
                            case "1": return "Partial";
                            case "2": return "Filled";
                            default: return execType;
                        }
                    }
                    return "Unknown";
                case "9":
                    return "Rejected";
                default:
                    return "Unknown";
            }
        }

        private string GetStateDetails(string msgType, JsonElement fields)
        {
            if (msgType == "8")
            {
                if (fields.TryGetProperty("150", out var execTypeElement))
                {
                    var execType = execTypeElement.GetString();
                    if (execType == "1" || execType == "2") // Partial or Filled
                    {
                        if (fields.TryGetProperty("32", out var lastQtyElement) &&
                            fields.TryGetProperty("31", out var lastPxElement))
                        {
                            return $"{lastQtyElement.GetString()}@{lastPxElement.GetString()}";
                        }
                    }
                    else if (execType == "8" && fields.TryGetProperty("58", out var textElement)) // Rejected
                    {
                        return textElement.GetString();
                    }
                }
            }
            else if (msgType == "9" && fields.TryGetProperty("58", out var textElement))
            {
                return textElement.GetString();
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