using DTO.Models.CommonModel;
using DTO.Models.Permission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OFMS_API.BL.Interface.Permission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OFMS_API.Controllers.Permission
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requires login
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionBL _permissionBL;

        public PermissionController(IPermissionBL permissionBL)
        {
            _permissionBL = permissionBL;
        }

        private (int userId, int roleId) GetCurrentUserIdentity()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst("Id")?.Value 
                         ?? User.FindFirst("userId")?.Value;
                         
            var roleIdStr = User.FindFirst(ClaimTypes.Role)?.Value 
                         ?? User.FindFirst("RoleId")?.Value 
                         ?? User.FindFirst("roleId")?.Value;

            int.TryParse(userIdStr, out int userId);
            int.TryParse(roleIdStr, out int roleId);

            return (userId, roleId);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyPermissions()
        {
            var response = new GlobalResponseModel<List<PermissionItemDTO>>();
            try
            {
                var (userId, roleId) = GetCurrentUserIdentity();
                var perms = await _permissionBL.GetMyEffectivePermissionsAsync(userId, roleId);

                response.data = perms;
                response.statusCode = StatusCodes.Status200OK;
                response.status = "Success";
                response.message = "Permissions retrieved successfully";
            }
            catch (Exception ex)
            {
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status = "Failed";
                response.message = ex.Message;
            }
            return Ok(response);
        }

        [HttpGet("menu")]
        public async Task<IActionResult> GetMyMenu()
        {
            var response = new GlobalResponseModel<List<tblModuleTO>>();
            try
            {
                var (userId, roleId) = GetCurrentUserIdentity();
                var menu = await _permissionBL.GetMyAllowedMenuAsync(userId, roleId);

                response.data = menu.OrderBy(m => m.DisplayOrder).ToList();
                response.statusCode = StatusCodes.Status200OK;
                response.status = "Success";
                response.message = "Menu retrieved successfully";
            }
            catch (Exception ex)
            {
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status = "Failed";
                response.message = ex.Message;
            }
            return Ok(response);
        }

        [HttpGet("matrix")]
        public async Task<IActionResult> GetRoleMatrix([FromQuery] int roleId)
        {
            var response = new GlobalResponseModel<List<ModulePermissionMatrixDTO>>();
            try
            {
                var matrix = await _permissionBL.GetRolePermissionMatrixAsync(roleId);
                response.data = matrix.OrderBy(m => m.DisplayOrder).ToList();
                response.statusCode = StatusCodes.Status200OK;
                response.status = "Success";
                response.message = "Matrix retrieved successfully";
            }
            catch (Exception ex)
            {
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status = "Failed";
                response.message = ex.Message;
            }
            return Ok(response);
        }

        [HttpPost("role-permissions")]
        public async Task<IActionResult> SaveRolePermissions([FromBody] SaveRolePermissionsDTO model)
        {
            var response = new GlobalResponseModel<bool>();
            try
            {
                var result = await _permissionBL.SaveRolePermissionsAsync(model);
                response.data = result;
                response.statusCode = StatusCodes.Status200OK;
                response.status = "Success";
                response.message = "Role permissions saved successfully";
            }
            catch (Exception ex)
            {
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status = "Failed";
                response.message = ex.Message;
            }
            return Ok(response);
        }

        [HttpGet("user-overrides")]
        public async Task<IActionResult> GetUserOverrides([FromQuery] int userId)
        {
            var response = new GlobalResponseModel<List<tblUserPermissionTO>>();
            try
            {
                var overrides = await _permissionBL.GetUserOverridesAsync(userId);
                response.data = overrides;
                response.statusCode = StatusCodes.Status200OK;
                response.status = "Success";
                response.message = "User overrides retrieved successfully";
            }
            catch (Exception ex)
            {
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status = "Failed";
                response.message = ex.Message;
            }
            return Ok(response);
        }

        [HttpPost("user-overrides")]
        public async Task<IActionResult> SaveUserOverrides([FromBody] SaveUserOverridesDTO model)
        {
            var response = new GlobalResponseModel<bool>();
            try
            {
                var result = await _permissionBL.SaveUserOverridesAsync(model);
                response.data = result;
                response.statusCode = StatusCodes.Status200OK;
                response.status = "Success";
                response.message = "User overrides saved successfully";
            }
            catch (Exception ex)
            {
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status = "Failed";
                response.message = ex.Message;
            }
            return Ok(response);
        }

        [HttpDelete("user-overrides")]
        public async Task<IActionResult> DeleteUserOverride([FromQuery] int userId, [FromQuery] int permissionId)
        {
            var response = new GlobalResponseModel<bool>();
            try
            {
                var result = await _permissionBL.DeleteUserOverrideAsync(userId, permissionId);
                response.data = result;
                response.statusCode = StatusCodes.Status200OK;
                response.status = "Success";
                response.message = "User override deleted successfully";
            }
            catch (Exception ex)
            {
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status = "Failed";
                response.message = ex.Message;
            }
            return Ok(response);
        }

        /// <summary>
        /// Returns the logged-in user's profile (info + role) using their JWT token.
        /// No parameters needed — userId is read from the token.
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var response = new GlobalResponseModel<object>();
            try
            {
                var (userId, roleId) = GetCurrentUserIdentity();
                if (userId == 0)
                {
                    response.statusCode = StatusCodes.Status401Unauthorized;
                    response.status = "Failed";
                    response.message = "Unable to identify user from token.";
                    return Ok(response);
                }

                // Use the existing user & address BL services via HttpClient-style direct DAL call
                // We'll return a simple anonymous object with what the frontend needs
                var userInfo = await _permissionBL.GetUserProfileAsync(userId);

                response.data = userInfo;
                response.statusCode = StatusCodes.Status200OK;
                response.status = "Success";
                response.message = "Profile retrieved successfully";
            }
            catch (Exception ex)
            {
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status = "Failed";
                response.message = ex.Message;
            }
            return Ok(response);
        }
    }
}
