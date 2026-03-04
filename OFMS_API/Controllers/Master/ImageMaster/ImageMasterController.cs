using DTO.Models.CommonModel;
using DTO.Models.Master.ImageMaster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.BL.Interface.Master.ImageMaster;

namespace OFMS_API.Controllers.Master.ImageMaster
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageMasterController : ControllerBase
    {
        private readonly IImageMasterBL _iImageMasterBL;

        public ImageMasterController(IImageMasterBL imageMasterBL)
        {
            _iImageMasterBL = imageMasterBL;
        }
         
        [HttpPost("AddItemMasterImage")] 
        public async Task<IActionResult> AddItemMasterImage([FromForm]TblImageMasterRequestTO tblImageMasterRequestTO)
        {
            var response = new GlobalResponseModel<ResultMessage>
            {
                message = "Image added successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (tblImageMasterRequestTO.ImageUrl == null || tblImageMasterRequestTO.ImageUrl.Length == 0
                || tblImageMasterRequestTO.ImageTypeId <= 0 || tblImageMasterRequestTO.ReferenceId <= 0)
            {
                response.message = "Invalid image data";
                response.status = "Fail";
                response.statusCode = StatusCodes.Status400BadRequest;
                return BadRequest(response);
            }

            try
            {
                var userIdClaim = User.FindFirst("userId");
                int? userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : null;

                if (userId == 0)
                {
                    response.message = "Unauthorized user";
                    response.status = "Fail";
                    response.statusCode = StatusCodes.Status401Unauthorized;
                    return Unauthorized(response);
                }

                tblImageMasterRequestTO.CreatedBy = Convert.ToInt32(userId);

                var result = await _iImageMasterBL.AddItemMasterImage(tblImageMasterRequestTO);

                if (result.IsSuccess == false)
                {
                    response.message = "Failed to add image";
                    response.status = "Error";
                    response.statusCode = StatusCodes.Status500InternalServerError;
                    response.data = result;
                    return Ok(response);
                }

                response.data = result;
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

        [HttpPost("AddItemMasterImageV2")]
        public async Task<IActionResult> AddItemMasterImageV2([FromForm] TblImageMasterRequestTO tblImageMasterRequestTO)
        {
            var response = new GlobalResponseModel<ResultMessage>
            {
                message = "Image added successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (tblImageMasterRequestTO.ImageUrl == null || tblImageMasterRequestTO.ImageUrl.Length == 0
                || tblImageMasterRequestTO.ImageTypeId <= 0 || tblImageMasterRequestTO.ReferenceId <= 0)
            {
                response.message = "Invalid image data";
                response.status = "Fail";
                response.statusCode = StatusCodes.Status400BadRequest;
                return BadRequest(response);
            }

            try
            {
                var userIdClaim = User.FindFirst("userId");
                int? userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : null;

                if (userId == 0)
                {
                    response.message = "Unauthorized user";
                    response.status = "Fail";
                    response.statusCode = StatusCodes.Status401Unauthorized;
                    return Unauthorized(response);
                }

                tblImageMasterRequestTO.CreatedBy = Convert.ToInt32(userId);

                var result = await _iImageMasterBL.AddItemMasterImage(tblImageMasterRequestTO);

                if (result.IsSuccess == false)
                {
                    response.message = "Failed to add image";
                    response.status = "Error";
                    response.statusCode = StatusCodes.Status500InternalServerError;
                    response.data = result;
                    return Ok(response);
                }

                response.data = result;
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


        [HttpPost("GetItemMasterImage")]
        public async Task<IActionResult> GetItemMasterImage([FromBody] FilterModelTO filter)
        {
            var response = new GlobalResponseModel<List<tblImageMasterResponseTO>>
            {
                message = "Images fetched successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            try
            {
                var userIdClaim = User.FindFirst("userId");
                int? userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : null;

                if (userId == 0)
                {
                    response.message = "Unauthorized user";
                    response.status = "Fail";
                    response.statusCode = StatusCodes.Status401Unauthorized;
                    return Unauthorized(response);
                }

                var result = await _iImageMasterBL.GetListOfItemMasterImage(filter);
                response.data = result.ToList();
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
         
        [HttpGet("GetItemMasterImageById/{id}")]
        public async Task<IActionResult> GetItemMasterImageById(int id)
        {
            var response = new GlobalResponseModel<tblImageMasterResponseTO>
            {
                message = "Image fetched successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (id <= 0)
            {
                response.message = "Invalid image id";
                response.status = "Fail";
                response.statusCode = StatusCodes.Status400BadRequest;
                return BadRequest(response);
            }

            try
            {
                var userIdClaim = User.FindFirst("userId");
                int? userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : null;

                if (userId == 0)
                {
                    response.message = "Unauthorized user";
                    response.status = "Fail";
                    response.statusCode = StatusCodes.Status401Unauthorized;
                    return Unauthorized(response);
                }

                var result = await _iImageMasterBL.GetItemMasterImageById(id);

                if (result == null)
                {
                    response.message = "Image not found";
                    response.status = "Fail";
                    response.statusCode = StatusCodes.Status404NotFound;
                    return NotFound(response);
                }

                response.data = result;
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
         
        [HttpPost("UpdateItemMasterImage")]
        public async Task<IActionResult> UpdateItemMasterImage([FromForm] TblImageMasterRequestTO tblImageMasterRequestTO)
        {
            var response = new GlobalResponseModel<ResultMessage>
            {
                message = "Image updated successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (tblImageMasterRequestTO.IdItemMasterImage <= 0
                || tblImageMasterRequestTO.ImageTypeId <= 0
                || tblImageMasterRequestTO.ReferenceId <= 0)
            {
                response.message = "Invalid image data";
                response.status = "Fail";
                response.statusCode = StatusCodes.Status400BadRequest;
                return BadRequest(response);
            }

            try
            {
                var userIdClaim = User.FindFirst("userId");
                int? userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : null;

                if (userId == 0)
                {
                    response.message = "Unauthorized user";
                    response.status = "Fail";
                    response.statusCode = StatusCodes.Status401Unauthorized;
                    return Unauthorized(response);
                }

                tblImageMasterRequestTO.UpdatedBy = Convert.ToInt32(userId);
                tblImageMasterRequestTO.UpdatedOn = DateTime.Now;

                var result = await _iImageMasterBL.UpdateItemMasterImage(tblImageMasterRequestTO);

                if (result.IsSuccess == false)
                {
                    response.message = "Failed to update image";
                    response.status = "Error";
                    response.statusCode = StatusCodes.Status500InternalServerError;
                    response.data = result;
                    return Ok(response);
                }

                response.data = result;
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
         
        [HttpDelete("DeleteItemMasterImage/{id}")]
        public async Task<IActionResult> DeleteItemMasterImage(int id)
        {
            var response = new GlobalResponseModel<ResultMessage>
            {
                message = "Image deleted successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (id <= 0)
            {
                response.message = "Invalid image id";
                response.status = "Fail";
                response.statusCode = StatusCodes.Status400BadRequest;
                return BadRequest(response);
            }

            try
            {
                var userIdClaim = User.FindFirst("userId");
                int? userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : null;

                if (userId == 0)
                {
                    response.message = "Unauthorized user";
                    response.status = "Fail";
                    response.statusCode = StatusCodes.Status401Unauthorized;
                    return Unauthorized(response);
                }

                var result = await _iImageMasterBL.DeleteItemMasterImage(id);

                if (result.IsSuccess == false)
                {
                    response.message = "Failed to delete image";
                    response.status = "Error";
                    response.statusCode = StatusCodes.Status500InternalServerError;
                    response.data = result;
                    return Ok(response);
                }

                response.data = result;
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