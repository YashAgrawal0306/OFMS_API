using DTO.Models.CommonModel;
using DTO.Models.Master.ThemeMaster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.BL.Interface.Master.ThemeMaster;
using System.Security.Claims;

namespace OFMS_API.Controllers.Master.ThemeMaster
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ThemeController(IThemeMasterBL db) : ControllerBase
    {
        // ── GET /api/Theme/GetUserTheme ────────────────────────────────────────
        [HttpGet("GetUserTheme")]
        public async Task<IActionResult> GetUserTheme()
        {
            var response = new GlobalResponseModel<TblThemeResponseTO>
            {
                message    = "Theme retrieved successfully",
                statusCode = StatusCodes.Status200OK,
                status     = "Success"
            };
            try
            {
                int userId = GetUserIdFromToken();
                if (userId <= 0)
                    return Unauthorized(new { message = "Invalid token" });

                var theme = await db.GetUserTheme(userId);
                response.data = theme; // null → frontend uses default
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.exception  = ex;
                response.message    = ex.Message;
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status     = "Error";
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        // ── POST /api/Theme/SaveUserTheme ──────────────────────────────────────
        [HttpPost("SaveUserTheme")]
        public async Task<IActionResult> SaveUserTheme([FromBody] TblThemeTO themeTO)
        {
            var response = new GlobalResponseModel<int>
            {
                message    = "Theme saved successfully",
                statusCode = StatusCodes.Status200OK,
                status     = "Success"
            };
            try
            {
                int userId = GetUserIdFromToken();
                if (userId <= 0)
                    return Unauthorized(new { message = "Invalid token" });

                int result = await db.SaveUserTheme(userId, themeTO);
                if (result <= 0)
                {
                    response.message    = "Failed to save theme";
                    response.statusCode = StatusCodes.Status400BadRequest;
                    response.status     = "Error";
                    return BadRequest(response);
                }

                response.data = result;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.exception  = ex;
                response.message    = ex.Message;
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status     = "Error";
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        // ── POST /api/Theme/ResetUserTheme ─────────────────────────────────────
        [HttpPost("ResetUserTheme")]
        public async Task<IActionResult> ResetUserTheme()
        {
            var response = new GlobalResponseModel<int>
            {
                message    = "Theme reset to default successfully",
                statusCode = StatusCodes.Status200OK,
                status     = "Success"
            };
            try
            {
                int userId = GetUserIdFromToken();
                if (userId <= 0)
                    return Unauthorized(new { message = "Invalid token" });

                int affected = await db.ResetUserTheme(userId);
                response.data = affected;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.exception  = ex;
                response.message    = ex.Message;
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status     = "Error";
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        // ── GET /api/Theme/GetThemeList ────────────────────────────────────────
        [HttpGet("GetThemeList")]
        public IActionResult GetThemeList()
        {
            var response = new GlobalResponseModel<List<ThemeColorItem>>
            {
                message    = "Theme list retrieved successfully",
                statusCode = StatusCodes.Status200OK,
                status     = "Success",
                data       = db.GetThemeList()
            };
            return Ok(response);
        }

        // ── Helper: extract userId from the JWT token ──────────────────────────
        private int GetUserIdFromToken()
        {
            // The JWT contains a claim "userId" set by the login service
            var userIdClaim = User.FindFirst("userId")
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)
                           ?? User.FindFirst("sub");

            if (userIdClaim == null) return 0;
            return int.TryParse(userIdClaim.Value, out int id) ? id : 0;
        }
    }
}
