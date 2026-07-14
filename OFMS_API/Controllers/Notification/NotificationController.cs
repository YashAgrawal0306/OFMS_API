using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DTO.Models.CommonModel;
using OFMS_API.Models;
using Services.BL.Interface.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OFMS_API.Controllers.Notification
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("GetMyNotifications")]
        public async Task<IActionResult> GetMyNotifications()
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new ResultMessage { Message = "Invalid user token.", IsSuccess = false });
                }

                var notifications = await _notificationService.GetUnreadNotificationsAsync(userId);
                return Ok(new ResultMessage { IsSuccess = true, Message = "Success", Data = notifications });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultMessage { Message = ex.Message, IsSuccess = false });
            }
        }

        [HttpPost("MarkAsRead/{notificationId}")]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            try
            {
                bool success = await _notificationService.MarkAsReadAsync(notificationId);
                return Ok(new ResultMessage { IsSuccess = success, Message = success ? "Notification marked as read" : "Failed to update" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultMessage { Message = ex.Message, IsSuccess = false });
            }
        }
    }
}
