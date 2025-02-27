using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FixMessageAnalyzer.Services;
using System.Threading.Tasks;
using FixMessageAnalyzer.Core.Services;
using Serilog;

namespace FixMessageAnalyzer.Api.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class FixFileController : ControllerBase
    {
        private readonly IFixFileService _fixFileService;

        public FixFileController(IFixFileService fixFileService)
        {
            _fixFileService = fixFileService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFixLog(IFormFileCollection files)
        {
            Log.Information("File upload initiated");
            try
            {
                if (files == null || !files.Any())
                    return BadRequest("No files uploaded.");

                var results = new List<object>();
                foreach (var file in files)
                {
                    Log.Information("Uploading file: {FileName}", file.FileName);
                    using var stream = file.OpenReadStream();
                    var sessionId = await _fixFileService.ProcessFixLogFileAsync(stream);
                    Log.Information("File processed successfully: {FileName}", file.FileName);
                    results.Add(new { sessionId, fileName = file.FileName });
                }

                return Ok(new { sessions = results, message = "Files processed successfully." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "File processing failed");
                return StatusCode(500, "File processing failed.");
            }
        }
    }
}
