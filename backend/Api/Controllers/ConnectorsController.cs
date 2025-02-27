using Microsoft.AspNetCore.Mvc;
using FixMessageAnalyzer.Core.Services;
using FixMessageAnalyzer.Data.DTOs;
using System.Threading.Tasks;
using FixMessageAnalyzer.Services;

namespace FixMessageAnalyzer.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConnectorsController : ControllerBase
    {
        private readonly IConnectorService _connectorService;
        private readonly ILogger<ConnectorsController> _logger;

        public ConnectorsController(
            IConnectorService connectorService,
            ILogger<ConnectorsController> logger)
        {
            _connectorService = connectorService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConnectorDto>>> GetConnectors(
            [FromQuery] string type = null)
        {
            try
            {
                var connectors = await _connectorService.GetConnectorsAsync(type);
                return Ok(connectors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving connectors");
                return StatusCode(500, "An error occurred while retrieving connectors");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ConnectorDto>> GetConnector(int id)
        {
            try
            {
                var connector = await _connectorService.GetConnectorAsync(id);
                if (connector == null)
                {
                    return NotFound();
                }
                return Ok(connector);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving connector {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the connector");
            }
        }

        [HttpPost]
        public async Task<ActionResult<ConnectorDto>> CreateConnector(CreateConnectorDto request)
        {
            try
            {
                var connector = await _connectorService.CreateConnectorAsync(request);
                return CreatedAtAction(nameof(GetConnector), new { id = connector.Id }, connector);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating connector");
                return StatusCode(500, "An error occurred while creating the connector");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateConnector(int id, UpdateConnectorDto request)
        {
            try
            {
                await _connectorService.UpdateConnectorAsync(id, request);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating connector {Id}", id);
                return StatusCode(500, "An error occurred while updating the connector");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConnector(int id)
        {
            try
            {
                await _connectorService.DeleteConnectorAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting connector {Id}", id);
                return StatusCode(500, "An error occurred while deleting the connector");
            }
        }

        [HttpPost("{id}/toggle")]
        public async Task<IActionResult> ToggleConnector(int id)
        {
            try
            {
                await _connectorService.ToggleConnectorAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling connector {Id}", id);
                return StatusCode(500, "An error occurred while toggling the connector");
            }
        }
    }
}