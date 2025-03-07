using FixMessageAnalyzer.Data;
using FixMessageAnalyzer.Data.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FixMessageAnalyzer.Core.Services
{
    public interface IFixMonitoringService
    {
        Task<MonitoringStatsDto> GetMonitoringStatsAsync(int userId);
    }

    public class FixMonitoringService : IFixMonitoringService
    {
        private readonly FixDbContext _dbContext;
        private const int LatencySampleSize = 1000;

        public FixMonitoringService(FixDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<MonitoringStatsDto> GetMonitoringStatsAsync(int userId)
        {
            var stats = new MonitoringStatsDto
            {
                MessageRates = await GetMessageRatesAsync(userId),
                LastMessageTimestamps = await GetLastMessageTimestampsAsync(userId),
                MessageTypes = await GetMessageTypeStatsAsync(userId),
                LatencyStats = await GetLatencyStatsAsync(userId),
                SessionHealth = await GetSessionHealthAsync(userId),
                SequenceResets = await GetSequenceResetsCountAsync(userId),
                RejectedMessages = await GetRejectedMessagesCountAsync(userId),
                ExchangeResets = await GetExchangeResetsCountAsync(userId),
                SessionMessages = await GetSessionMessagesCountAsync(userId)
            };

            return stats;
        }

        private async Task<List<MessageRateDto>> GetMessageRatesAsync(int userId)
        {
            var query = _dbContext.Messages.AsQueryable();

            query = query.Where(m => m.UserId == userId);

            return await query
                .GroupBy(m => new { m.SenderCompID, m.TargetCompID })
                .Select(g => new MessageRateDto
                {
                    SessionKey = g.Key.SenderCompID + "->" + g.Key.TargetCompID,
                    MessageCount = g.Count()
                })
                .ToListAsync();
        }

        private async Task<List<LastMessageTimestampDto>> GetLastMessageTimestampsAsync(int userId)
        {
            var query = _dbContext.Messages.AsQueryable();

            query = query.Where(m => m.UserId == userId);

            return await query
                .GroupBy(m => new { m.SenderCompID, m.TargetCompID })
                .Select(g => new LastMessageTimestampDto
                {
                    SessionKey = g.Key.SenderCompID + "->" + g.Key.TargetCompID,
                    LastTimestamp = g.Max(m => m.Timestamp)
                })
                .ToListAsync();
        }

        private async Task<MessageTypeStatsDto> GetMessageTypeStatsAsync(int userId)
        {
            var today = DateTime.UtcNow.Date;
            var query = _dbContext.Messages.AsQueryable();

            query = query.Where(m => m.UserId == userId);

            var messageTypes = await query
                .Where(m => m.Timestamp >= today)
                .GroupBy(m => m.MsgType)
                .Select(g => new { MsgType = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.MsgType, x => x.Count);

            return new MessageTypeStatsDto
            {
                NewOrders = await GetNewOrdersCountAsync(today, userId),
                RestatedOrders = await GetRestatedOrdersCountAsync(userId),
                CancelledOrders = await GetCancelledOrdersCountAsync(userId),
                MessageTypeDistribution = messageTypes
            };
        }

        private async Task<int> GetNewOrdersCountAsync(DateTime today, int userId)
        {
            var query = _dbContext.Messages.Where(m => m.MsgType == "D" && m.Timestamp >= today);

            query = query.Where(m => m.UserId == userId);

            return await query.CountAsync();
        }

        private async Task<int> GetRestatedOrdersCountAsync(int userId)
        {
            var query = _dbContext.Messages.Where(m => m.MsgType == "D" &&
                           EF.Functions.JsonExists(m.Fields, "PossDupFlag") &&
                           EF.Functions.JsonContains(m.Fields, @"{""PossDupFlag"": ""Y""}"));

            query = query.Where(m => m.UserId == userId);

            return await query.CountAsync();
        }

        private async Task<int> GetCancelledOrdersCountAsync(int userId)
        {
            var query = _dbContext.Messages.Where(m => m.MsgType == "F" ||
                          (m.MsgType == "8" &&
                           EF.Functions.JsonExists(m.Fields, "ExecType") &&
                           EF.Functions.JsonContains(m.Fields, @"{""ExecType"": ""4""}")));

            query = query.Where(m => m.UserId == userId);

            return await query.CountAsync();
        }

        private async Task<LatencyStatsDto> GetLatencyStatsAsync(int userId)
        {
            var query = _dbContext.Messages.Where(m => EF.Functions.JsonExists(m.Fields, "TransactTime"));

            query = query.Where(m => m.UserId == userId);

            var latencyData = await query
                .OrderByDescending(m => m.Timestamp)
                .Take(LatencySampleSize)
                .Select(m => new LatencyDataItem
                {
                    SendTime = m.Timestamp,
                    TransactTime = m.Fields["TransactTime"] // Accessing JSONB dictionary directly
                })
                .ToListAsync();

            var latencies = GetLatenciesFromData(latencyData);

            return new LatencyStatsDto
            {
                AverageLatencyMs = latencies.Any() ? latencies.Average() : 0,
                P95LatencyMs = latencies.Any() ? latencies[(int)(latencies.Count * 0.95)] : 0,
                P99LatencyMs = latencies.Any() ? latencies[(int)(latencies.Count * 0.99)] : 0
            };
        }

        private class LatencyDataItem
        {
            public DateTime SendTime { get; set; }
            public string TransactTime { get; set; }
        }

        private List<double> GetLatenciesFromData(List<LatencyDataItem> latencyData)
        {
            return latencyData
                .Select(m =>
                {
                    if (DateTime.TryParse(m.TransactTime, out DateTime transactTime))
                    {
                        return Math.Abs((m.SendTime - transactTime).TotalMilliseconds);
                    }
                    return 0.0;
                })
                .Where(l => l > 0)
                .OrderBy(l => l)
                .ToList();
        }

        private List<double> GetLatenciesFromData(IEnumerable<dynamic> latencyData)
        {
            return latencyData
                .Select(m =>
                {
                    if (DateTime.TryParse(m.TransactTime.ToString(), out DateTime transactTime))
                    {
                        var sendTime = (DateTime)m.SendTime;
                        return Math.Abs((sendTime - transactTime).TotalMilliseconds);
                    }
                    return 0.0;
                })
                .Where(l => l > 0)
                .OrderBy(l => l)
                .ToList();
        }

        private async Task<SessionHealthDto> GetSessionHealthAsync(int userId)
        {
            var sessions = await GetSessionMessagesAsync(userId);
            var sessionGaps = await CalculateSequenceGapsAsync(sessions);
            var totalGaps = sessionGaps.Values.Sum(gaps => gaps.Count);

            // Get total message count for health score calculation
            var query = _dbContext.Messages.AsQueryable();
            query = query.Where(m => m.UserId == userId);
            var totalMessages = await query.CountAsync();

            var sequenceResets = await GetSequenceResetsCountAsync(userId);

            return new SessionHealthDto
            {
                TotalGaps = totalGaps,
                SequenceGaps = sessionGaps,
                HealthScore = CalculateHealthScore(totalGaps, sequenceResets, totalMessages)
            };
        }

        private class SessionMessageInfo
        {
            public string SessionId { get; set; }
            public List<MessageSequenceInfo> Messages { get; set; }
        }

        private class MessageSequenceInfo
        {
            public int SequenceNumber { get; set; }
            public DateTime Timestamp { get; set; }
        }

        private async Task<List<SessionMessageInfo>> GetSessionMessagesAsync(int userId)
        {
            var query = _dbContext.Messages.AsQueryable();

            query = query.Where(m => m.UserId == userId);

            return await query
                .GroupBy(m => m.SessionId)
                .Select(g => new SessionMessageInfo
                {
                    SessionId = g.Key,
                    Messages = g.OrderBy(m => m.SequenceNumber)
                               .Select(m => new MessageSequenceInfo
                               {
                                   SequenceNumber = m.SequenceNumber,
                                   Timestamp = m.Timestamp
                               })
                               .ToList()
                })
                .ToListAsync();
        }

        private async Task<Dictionary<string, List<SequenceGap>>> CalculateSequenceGapsAsync(List<SessionMessageInfo> sessions)
        {
            var sessionGaps = new Dictionary<string, List<SequenceGap>>();

            foreach (var session in sessions)
            {
                var gaps = new List<SequenceGap>();
                var messages = session.Messages;

                for (int i = 1; i < messages.Count; i++)
                {
                    var expected = messages[i - 1].SequenceNumber + 1;
                    var actual = messages[i].SequenceNumber;

                    if (actual > expected)
                    {
                        gaps.Add(new SequenceGap
                        {
                            ExpectedSeqNum = expected,
                            ReceivedSeqNum = actual,
                            DetectedAt = messages[i].Timestamp
                        });
                    }
                }

                if (gaps.Any())
                {
                    sessionGaps[session.SessionId] = gaps;
                }
            }

            return sessionGaps;
        }

        private async Task<Dictionary<string, List<SequenceGap>>> CalculateSequenceGapsAsync(List<dynamic> sessions)
        {
            var sessionGaps = new Dictionary<string, List<SequenceGap>>();

            foreach (var session in sessions)
            {
                var gaps = new List<SequenceGap>();
                var messages = session.Messages.ToList();

                for (int i = 1; i < messages.Count; i++)
                {
                    var expected = messages[i - 1].SequenceNumber + 1;
                    var actual = messages[i].SequenceNumber;

                    if (actual > expected)
                    {
                        gaps.Add(new SequenceGap
                        {
                            ExpectedSeqNum = expected,
                            ReceivedSeqNum = actual,
                            DetectedAt = messages[i].Timestamp
                        });
                    }
                }

                if (gaps.Any())
                {
                    sessionGaps[session.SessionId] = gaps;
                }
            }

            return sessionGaps;
        }

        private double CalculateHealthScore(int totalGaps, int sequenceResets, int totalMessages)
        {
            if (totalMessages == 0) return 100;
            return Math.Max(0, 100 - ((totalGaps + sequenceResets) / (double)totalMessages * 100));
        }

        private async Task<int> GetSequenceResetsCountAsync(int? userId = null)
        {
            var query = _dbContext.Messages.Where(m => m.MsgType == "4");

            query = query.Where(m => m.UserId == userId);

            return await query.CountAsync();
        }

        private async Task<int> GetRejectedMessagesCountAsync(int? userId = null)
        {
            var query = _dbContext.Messages.Where(m => m.MsgType == "8" &&
                       EF.Functions.JsonExists(m.Fields, "ExecType") &&
                       EF.Functions.JsonContains(m.Fields, @"{""ExecType"": ""8""}"));

            query = query.Where(m => m.UserId == userId);

            return await query.CountAsync();
        }

        private async Task<int> GetExchangeResetsCountAsync(int userId)
        {
            var query = _dbContext.Messages.Where(m => m.MsgType == "4" &&
                               m.SenderCompID.StartsWith("EXCHANGE"));

            query = query.Where(m => m.UserId == userId);

            return await query.CountAsync();
        }

        private async Task<int> GetSessionMessagesCountAsync(int userId)
        {
            var query = _dbContext.Messages.Where(m => m.MsgType == "A" ||
                               m.MsgType == "5" ||
                               m.MsgType == "0");

            query = query.Where(m => m.UserId == userId);

            return await query.CountAsync();
        }
    }
}