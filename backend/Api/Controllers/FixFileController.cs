using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FixMessageAnalyzer.Services;
using System.Threading.Tasks;
using FixMessageAnalyzer.Core.Services;
using Serilog;
using Microsoft.AspNetCore.Authorization;

namespace FixMessageAnalyzer.Api.Controllers
{
    [Route("api/files")]
    [Authorize]
    [ApiController]
    public class FixFileController : BaseApiController
    {
        private readonly IFixFileService _fixFileService;
        private readonly IFixDictionaryService _dictionaryService;

        public FixFileController(
            IFixFileService fixFileService,
            IFixDictionaryService dictionaryService)
        {
            _fixFileService = fixFileService;
            _dictionaryService = dictionaryService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFixLog(
            IFormFileCollection files,
            [FromForm] string fixVersion = null)
        {
            Log.Information("File upload initiated");
            try
            {
                if (files == null || !files.Any())
                    return BadRequest("No files uploaded.");

                // Validate version if provided
                if (!string.IsNullOrEmpty(fixVersion) &&
                    !_dictionaryService.GetSupportedVersions().Contains(fixVersion))
                {
                    return BadRequest($"Unsupported FIX version: {fixVersion}");
                }

                var results = new List<object>();
                foreach (var file in files)
                {
                    Log.Information("Uploading file: {FileName}, Version: {Version}", file.FileName, fixVersion ?? "auto-detect");
                    using var stream = file.OpenReadStream();
                    var sessionId = await _fixFileService.ProcessFixLogFileAsync(stream, GetCurrentUserId(), fixVersion);
                    Log.Information("File processed successfully: {FileName}", file.FileName);
                    results.Add(new { sessionId, fileName = file.FileName, fixVersion = fixVersion ?? "auto-detect" });
                }

                return Ok(new
                {
                    sessions = results,
                    message = "Files processed successfully."
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "File processing failed");
                return StatusCode(500, $"File processing failed: {ex.Message}");
            }
        }

        [HttpGet("supported-versions")]
        public IActionResult GetSupportedVersions()
        {
            return Ok(new { versions = _dictionaryService.GetSupportedVersions() });
        }
    }
}
