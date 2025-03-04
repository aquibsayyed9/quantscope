// FixMessageController.cs
using Microsoft.AspNetCore.Mvc;
using FixMessageAnalyzer.Services;
using System;
using System.Threading.Tasks;

namespace FixMessageAnalyzer.Api.Controllers
{
    [Route("api/messages")]
    [ApiController]
    public class FixMessageController : ControllerBase
    {
        private readonly IFixMessageService _fixMessageService;

        public FixMessageController(IFixMessageService fixMessageService)
        {
            _fixMessageService = fixMessageService ?? throw new ArgumentNullException(nameof(fixMessageService));
        }

        [HttpGet]
        public async Task<IActionResult> GetMessages(
            [FromQuery] string[]? msgTypes = null,
            [FromQuery] string? orderId = null,
            [FromQuery] DateTime? startTime = null,
            [FromQuery] DateTime? endTime = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100,
            [FromQuery] bool skipHeartbeats = false)
        {
            try
            {
                var messages = await _fixMessageService.GetMessagesAsync(
                    msgTypes,
                    orderId,
                    startTime,
                    endTime,                    
                    page,
                    pageSize,
                    skipHeartbeats);

                return Ok(messages);
            }
            catch (Exception ex)
            {
                // Log the exception details here
                return StatusCode(500, "An error occurred while fetching messages");
            }
        }
    }
}