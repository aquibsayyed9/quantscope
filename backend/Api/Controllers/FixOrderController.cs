using Microsoft.AspNetCore.Mvc;
using FixMessageAnalyzer.Services;

namespace FixMessageAnalyzer.Api.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class FixOrderController : ControllerBase
    {
        private readonly IFixOrderService _fixOrderService;

        public FixOrderController(IFixOrderService fixOrderService)
        {
            _fixOrderService = fixOrderService;
        }

        //[HttpGet("{orderId}/flow")]
        //public async Task<IActionResult> GetOrderFlow(string orderId)
        //{
        //    var orderFlow = await _fixOrderService.GetOrderFlowAsync(orderId);
        //    return Ok(orderFlow);
        //}
    }
}
