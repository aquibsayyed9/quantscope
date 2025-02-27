using Microsoft.AspNetCore.Mvc;
using FixMessageAnalyzer.Services;
using System.Threading.Tasks;
using FixMessageAnalyzer.Core.Services;

namespace FixMessageAnalyzer.Api.Controllers
{
    [Route("api/sessions")]
    [ApiController]
    public class FixSessionController : ControllerBase
    {
        private readonly IFixSessionService _fixSessionService;

        public FixSessionController(IFixSessionService fixSessionService)
        {
            _fixSessionService = fixSessionService;
        }

        //[HttpGet]
        //public async Task<IActionResult> GetSessions()
        //{
        //    var sessions = await _fixSessionService.GetAllSessionsAsync();
        //    return Ok(sessions);
        //}

        [HttpGet("{sessionId}")]
        public async Task<IActionResult> GetSessionDetails(long sessionId)
        {
            var sessionDetails = await _fixSessionService.GetSessionDetailsAsync(sessionId);
            if (sessionDetails == null)
                return NotFound("Session not found.");

            return Ok(sessionDetails);
        }
    }
}
