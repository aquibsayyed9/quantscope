using FixMessageAnalyzer.Services;
using FixMessageAnalyzer.Data.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FixMessageAnalyzer.Api.Controllers;

namespace FixMessageAnalyzer.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class OrderFlowController : BaseApiController
    {
        private readonly IFixOrderService _orderService;

        public OrderFlowController(IFixOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Get order flow information with flexible identifier tracking
        /// </summary>
        /// <param name="orderId">Exchange/Broker assigned OrderID (tag 37)</param>
        /// <param name="clOrdId">Client assigned ClOrdID (tag 11)</param>
        /// <param name="trackingMode">Mode for tracking orders: "OrderId" or "ClOrdId"</param>
        /// <param name="symbol">Instrument symbol</param>
        /// <param name="startDate">Filter start date</param>
        /// <param name="endDate">Filter end date</param>
        /// <param name="pageSize">Number of records per page</param>
        /// <param name="pageNumber">Page number</param>
        [HttpGet]
        public async Task<ActionResult<OrderFlowResponse>> GetOrderFlow(
            [FromQuery] string? orderId = null,
            [FromQuery] string? clOrdId = null,
            [FromQuery] OrderTrackingMode? trackingMode = null,
            [FromQuery] string? symbol = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? pageSize = 10,
            [FromQuery] int? pageNumber = 1)
        {
            // If tracking mode is not specified, default it to OrderId
            if (trackingMode == null)
            {
                trackingMode = OrderTrackingMode.OrderId;
            }

            // Validate tracking mode matches provided identifier
            if (trackingMode == OrderTrackingMode.ClOrdId && string.IsNullOrEmpty(clOrdId) && !string.IsNullOrEmpty(orderId))
            {
                return BadRequest("ClOrdId must be provided when using ClOrdId tracking mode");
            }
            if (trackingMode == OrderTrackingMode.OrderId && string.IsNullOrEmpty(orderId) && !string.IsNullOrEmpty(clOrdId))
            {
                return BadRequest("OrderId must be provided when using OrderId tracking mode");
            }

            var filter = new OrderFlowFilterDto
            {
                OrderId = orderId,
                ClOrdId = clOrdId,
                TrackingMode = trackingMode,
                Symbol = symbol,
                StartDate = startDate,
                EndDate = endDate,
                PageSize = pageSize,
                PageNumber = pageNumber
            };

            var result = await _orderService.GetOrderFlowAsync(GetCurrentUserId(), filter);
            return Ok(result);
        }

        /// <summary>
        /// Legacy endpoint maintained for backward compatibility
        /// </summary>
        [HttpGet("legacy")]
        public async Task<ActionResult<OrderFlowResponse>> GetOrderFlowLegacy(
            [FromQuery] string? orderId,
            [FromQuery] string? symbol,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int? pageSize = 10,
            [FromQuery] int? pageNumber = 1)
        {
            var filter = new OrderFlowFilterDto
            {
                OrderId = orderId,
                Symbol = symbol,
                StartDate = startDate,
                EndDate = endDate,
                PageSize = pageSize,
                PageNumber = pageNumber,
                TrackingMode = OrderTrackingMode.OrderId // Force OrderId mode for backward compatibility
            };

            var result = await _orderService.GetOrderFlowAsync(GetCurrentUserId(), filter);
            return Ok(result);
        }
    }
}