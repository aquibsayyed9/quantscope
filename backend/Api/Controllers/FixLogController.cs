using FixMessageAnalyzer.Core.Services;
using FixMessageAnalyzer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FixMessageAnalyzer.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/fixlog")]
    public class FixLogController : BaseApiController
    {
        private readonly IFixLogParsingService _fixLogParser;
        private readonly IFixMonitoringService _monitoringService;

        public FixLogController(IFixLogParsingService fixLogParser, IFixMonitoringService monitoringService)
        {
            _fixLogParser = fixLogParser;
            _monitoringService = monitoringService;
        }

        [HttpGet("monitoring")]
        public async Task<IActionResult> GetMonitoringStats()
        {
            try
            {
                var stats = await _monitoringService.GetMonitoringStatsAsync(GetCurrentUserId());
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving monitoring stats", details = ex.Message });
            }
        }
    }
}
