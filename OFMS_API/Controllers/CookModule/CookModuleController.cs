using DTO.Models.CommonModel;
using DTO.Models.CookModule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OFMS_API.Services.BL.Interface.CookModule;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OFMS_API.Controllers.CookModule
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Enforce login
    public class CookModuleController : ControllerBase
    {
        private readonly ICookModuleBL _cookModuleBL;

        public CookModuleController(ICookModuleBL cookModuleBL)
        {
            _cookModuleBL = cookModuleBL;
        }

        private int GetCookUserId()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Invalid or missing Cook authentication token.");
        }

        [HttpGet("Dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                int cookUserId = GetCookUserId();
                var data = await _cookModuleBL.GetCookDashboardCounts(cookUserId);
                var response = new GlobalResponseModel<CookDashboardCountsTO>
                {
                    message = "Dashboard counts retrieved successfully",
                    statusCode = StatusCodes.Status200OK,
                    data = data
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("MyAssignedOrders")]  
        public async Task<IActionResult> GetMyAssignedOrders([FromBody] FilterModelTO filter)
        {
            try
            {
                int cookUserId = GetCookUserId();
                var data = await _cookModuleBL.GetMyAssignedOrders(cookUserId, filter, false);
                var response = new GlobalResponseModel<OutPutClass<CookOrderListResponseTO>>
                {
                    message = "Orders retrieved successfully",
                    statusCode = StatusCodes.Status200OK,
                    data = data
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("CompletedHistory")]
        public async Task<IActionResult> GetCompletedHistory([FromBody] FilterModelTO filter)
        {
            try
            {
                int cookUserId = GetCookUserId();
                var data = await _cookModuleBL.GetMyAssignedOrders(cookUserId, filter, true);
                var response = new GlobalResponseModel<OutPutClass<CookOrderListResponseTO>>
                {
                    message = "Completed history retrieved successfully",
                    statusCode = StatusCodes.Status200OK,
                    data = data
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("OrderDetails/{orderId}")]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            try
            {
                int cookUserId = GetCookUserId();
                var data = await _cookModuleBL.GetOrderDetailsForCook(cookUserId, orderId);
                
                if (data == null)
                    return Unauthorized(new { message = "You are not authorized to view this order or it does not exist." });

                var response = new GlobalResponseModel<CookOrderDetailResponseTO>
                {
                    message = "Order details retrieved successfully",
                    statusCode = StatusCodes.Status200OK,
                    data = data
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("AcceptOrder")]
        public async Task<IActionResult> AcceptOrder([FromBody] AcceptOrderRequestTO request)
        {
            try
            {
                int cookUserId = GetCookUserId();
                bool success = await _cookModuleBL.AcceptOrder(cookUserId, request);
                
                if (!success)
                    return BadRequest(new { message = "Failed to accept order. It might already be accepted or belongs to someone else." });

                return Ok(new GlobalResponseModel<bool>
                {
                    message = "Order accepted successfully",
                    statusCode = StatusCodes.Status200OK,
                    data = success
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("UpdateStatus")]
        public async Task<IActionResult> UpdateCookingStatus([FromBody] UpdateCookingStatusRequestTO request)
        {
            try
            {
                int cookUserId = GetCookUserId();
                bool success = await _cookModuleBL.UpdateCookingStatus(cookUserId, request);
                
                if (!success)
                    return BadRequest(new { message = "Failed to update status. Validation failed." });

                return Ok(new GlobalResponseModel<bool>
                {
                    message = "Status updated successfully",
                    statusCode = StatusCodes.Status200OK,
                    data = success
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("UpdateEstimatedTime")]
        public async Task<IActionResult> UpdateEstimatedTime([FromBody] UpdateEstimatedTimeRequestTO request)
        {
            try
            {
                int cookUserId = GetCookUserId();
                bool success = await _cookModuleBL.UpdateEstimatedTime(cookUserId, request);
                
                if (!success)
                    return BadRequest(new { message = "Failed to update estimated time." });

                return Ok(new GlobalResponseModel<bool>
                {
                    message = "Estimated time updated successfully",
                    statusCode = StatusCodes.Status200OK,
                    data = success
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
