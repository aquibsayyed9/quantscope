using FixMessageAnalyzer.Core.Services;
using FixMessageAnalyzer.Services;
using Microsoft.AspNetCore.Mvc;

namespace FixMessageAnalyzer.Api.Controllers
{
    [ApiController]
    [Route("api/fixlog")]
    public class FixLogController : ControllerBase
    {
        private readonly IFixLogParsingService _fixLogParser;
        private readonly IFixMonitoringService _monitoringService;

        public FixLogController(IFixLogParsingService fixLogParser, IFixMonitoringService monitoringService)
        {
            _fixLogParser = fixLogParser;
            _monitoringService = monitoringService;
        }

        //[HttpGet("parse")]
        //public IActionResult ParseLog()
        //{
        //    string filePath = "C:\\pws\\FixMessageAnalyzer\\FixLogFiles\\FSRA_FSRA_main.out";
        //    IEnumerable<FixMessage> parsedData = _fixLogParser.ReadFixLog(filePath);
        //    return Ok(parsedData);
        //}

        [HttpGet("monitoring")]
        public async Task<IActionResult> GetMonitoringStats()
        {
            try
            {
                var stats = await _monitoringService.GetMonitoringStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving monitoring stats", details = ex.Message });
            }
        }
    }
}
