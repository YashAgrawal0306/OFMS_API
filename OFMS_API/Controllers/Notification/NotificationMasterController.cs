using DTO.Models.CommonModel;
using DTO.Models.Notification;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.BL.Interface.Notification;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OFMS_API.Controllers.Notification
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationMasterController : ControllerBase
    {
        private readonly INotificationMasterBL _bl;

        public NotificationMasterController(INotificationMasterBL bl)
        {
            _bl = bl;
        }

        [HttpGet("GetPermissionsByRole")]
        public async Task<IActionResult> GetPermissionsByRole(int roleId)
        {
            var response = new GlobalResponseModel<List<NotificationMasterResponseTO>>();
            try
            {
                var data = await _bl.GetNotificationPermissionsByRole(roleId);
                response.statusCode = StatusCodes.Status200OK;
                response.status = "Success";
                response.message = "Permissions fetched successfully";
                response.data = data;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.message = ex.Message;
                response.exception = ex;
                response.status = "Error";
                response.statusCode = StatusCodes.Status500InternalServerError;
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpPost("UpdatePermissions")]
        public async Task<IActionResult> UpdatePermissions([FromBody] List<NotificationRoleMappingTO> mappings)
        {
            var response = new GlobalResponseModel<bool>();
            try
            {
                bool success = await _bl.UpdateNotificationPermissions(mappings);
                response.statusCode = StatusCodes.Status200OK;
                response.status = "Success";
                response.message = "Permissions updated successfully";
                response.data = success;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.message = ex.Message;
                response.exception = ex;
                response.status = "Error";
                response.statusCode = StatusCodes.Status500InternalServerError;
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
    }
}
