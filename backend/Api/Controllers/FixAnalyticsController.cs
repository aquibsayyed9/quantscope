using Microsoft.AspNetCore.Mvc;
using FixMessageAnalyzer.Services;
using System.Threading.Tasks;

namespace FixMessageAnalyzer.Api.Controllers
{
    [Route("api/analytics")]
    [ApiController]
    public class FixAnalyticsController : ControllerBase
    {
        private readonly IFixAnalyticsService _fixAnalyticsService;

        public FixAnalyticsController(IFixAnalyticsService fixAnalyticsService)
        {
            _fixAnalyticsService = fixAnalyticsService;
        }

        [HttpGet("message-types")]
        public async Task<IActionResult> GetMessageTypeDistribution()
        {
            var result = await _fixAnalyticsService.GetSymbolDistributionAsync();
            return Ok(result);
        }

        [HttpGet("symbols")]
        public async Task<IActionResult> GetSymbolDistribution()
        {
            var result = await _fixAnalyticsService.GetSymbolDistributionLinqAsync();
            return Ok(result);
        }
    }
}
