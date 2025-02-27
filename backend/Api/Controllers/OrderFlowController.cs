﻿using FixMessageAnalyzer.Services;
using FixMessageAnalyzer.Data.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FixMessageAnalyzer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderFlowController : ControllerBase
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
            // Validate identifier usage
            if (string.IsNullOrEmpty(orderId) && string.IsNullOrEmpty(clOrdId) && !string.IsNullOrEmpty(symbol))
            {
                // Allow search by symbol only
            }
            else if (string.IsNullOrEmpty(orderId) && string.IsNullOrEmpty(clOrdId))
            {
                return BadRequest("Either OrderId or ClOrdId must be provided unless searching by symbol");
            }

            // If tracking mode is not specified, infer it from the provided identifiers
            if (trackingMode == null)
            {
                trackingMode = !string.IsNullOrEmpty(clOrdId)
                    ? OrderTrackingMode.ClOrdId
                    : OrderTrackingMode.OrderId;
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

            var result = await _orderService.GetOrderFlowAsync(filter);
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

            var result = await _orderService.GetOrderFlowAsync(filter);
            return Ok(result);
        }
    }
}