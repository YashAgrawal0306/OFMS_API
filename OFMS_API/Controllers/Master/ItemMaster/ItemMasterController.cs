using DTO.Models.CommonModel;
using DTO.Models.Master.ItemMaster;
using DTO.Models.Master.ItemMaster.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using OFMS_API.Models;
using Services.BL.Interface.Master.ItemMaster;

namespace OFMS_API.Controllers.Master.ItemMaster
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemMasterController : ControllerBase
    {
        private readonly IItemMasterBL _IItemMasterBL;

        public ItemMasterController(IItemMasterBL itemMasterBL)
        {
            _IItemMasterBL = itemMasterBL;
        }

        #region Group Master
        [HttpPost("GetAllGroupdMasterList")]
        public async Task<IActionResult> GetGroupdMasterList(FilterModelTO filterModelTO)
        {
            var response = new GlobalResponseModel<IEnumerable<TblGroupMasterResponseTO>>
            {
                message = "Groups fetched successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            try
            {
                var data = await _IItemMasterBL.GetListOfGroupMaster(filterModelTO);
                response.data = data.Item1;
                response.TotalRecords =  data.Item2;
                

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.message =  ex.Message;
                response.exception = ex;
                response.status = "Error addd";
                response.statusCode = StatusCodes.Status500InternalServerError;

                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
        [HttpPost("GetGroupdMasterListNew")]
        public async Task<IActionResult> GetGroupdMasterListNew(FilterModelTO filterModelTO)
        {
            var response = new GlobalResponseModel<IEnumerable<TblGroupMasterResponseTO>>
            {
                message = "Groups fetched successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            try
            {
                var data = await _IItemMasterBL.GetListOfGroupMaster(filterModelTO);
                response.data = data.Item1;
                response.TotalRecords = data.Item2;


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

        [HttpPost("AddGroupMaster")]
        public async Task<IActionResult> AddGroupMaster([FromBody] TblGroupMasterTO model)
        {
            var response = new GlobalResponseModel<ResultMessage>
            {
                message = "Group added successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (model == null || string.IsNullOrWhiteSpace(model.GroupName))
            {
                response.message = "Invalid group data";
                response.status = "Fail";
                response.statusCode = StatusCodes.Status400BadRequest;
                return BadRequest(response);
            }
            try
            {
                var userIdClaim = User.FindFirst("userId");
                int? Userid = userIdClaim != null ? int.Parse(userIdClaim.Value) : null;
                if (Userid == 0)
                {
                    response.message = "Unauthorized user";
                    response.status = "Fail";
                    response.statusCode = StatusCodes.Status401Unauthorized;
                    return Unauthorized(response);
                }
                model.CreatedBy = Convert.ToInt32(Userid);
                var result = await _IItemMasterBL.AddGroupMaster(model);

                if (result.IsSuccess == false)
                {
                    response.message = "Failed to add group";
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

        [HttpPut("UpdateGroupMaster")]
        public async Task<IActionResult> UpdateGroupMaster([FromBody] TblGroupMasterTO model)
        {
            var response = new GlobalResponseModel<ResultMessage>
            {
                message = "Group updated successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            try
            {
                var userIdClaim = User.FindFirst("userId");
                int? Userid = userIdClaim != null ? int.Parse(userIdClaim.Value) : null;
                if (Userid == 0)
                {
                    response.message = "Unauthorized user";
                    response.status = "Fail";
                    response.statusCode = StatusCodes.Status401Unauthorized;
                    return Unauthorized(response);
                }
                model.UpdatedBy = Userid;
                var result = await _IItemMasterBL.UpdateGroupMaster(model);

                if (result.IsSuccess == false)
                {
                    response.message = "Failed to update group";
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

        [HttpDelete("DeleteGroupMaster")]
        public async Task<IActionResult> DeleteGroupMaster(int idGroup)
        {
            var response = new GlobalResponseModel<ResultMessage>
            {
                message = "group deleted successfully",
                statusCode = StatusCodes.Status200OK,
                status = "success"
            };

            try
            {
                if (idGroup <= 0)
                {
                    response.message = "Invalid group ID";
                    response.status = "fail";
                    response.statusCode = StatusCodes.Status400BadRequest;
                    return BadRequest(response);
                }

                var result = await _IItemMasterBL.DeleteGroupMaster(idGroup);

                if (result.IsSuccess == false)
                {
                    response.message = "failed to delete group";
                    response.status = "error";
                    response.statusCode = StatusCodes.Status500InternalServerError;
                    return Ok(response);
                }

                response.data = result;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.message = ex.Message;
                response.exception = ex;
                response.status = "error";
                response.statusCode = StatusCodes.Status500InternalServerError;
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpGet("GetGroupById")]
        public async Task<IActionResult> GetGroupById(int idGrop)
        {
            var response = new GlobalResponseModel<TblGroupMasterTO>
            {
                message = "Group fetched successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            try
            {
                var data = await _IItemMasterBL.GetGroupById(idGrop);

                if (data == null)
                {
                    response.message = "Group not found";
                    response.status = "Fail";
                    response.statusCode = StatusCodes.Status404NotFound;
                    return Ok(response);
                }

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
        #endregion

        #region Category Master

        [HttpPost("AddCategoryMaster")]
        public async Task<IActionResult> AddCategoryMaster([FromBody] TblCategoryMasterTO model)
        {
            var response = new GlobalResponseModel<ResultMessage>
            {
                message = "Category added successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (model == null || string.IsNullOrWhiteSpace(model.CategoryName))
            {
                response.message = "Invalid category data";
                response.status = "Fail";
                response.statusCode = StatusCodes.Status400BadRequest;
                return BadRequest(response);
            }

            try
            {
                var userIdClaim = User.FindFirst("userId");
                int? Userid = userIdClaim != null ? int.Parse(userIdClaim.Value) : null;

                if (Userid == 0)
                {
                    response.message = "Unauthorized user";
                    response.status = "Fail";
                    response.statusCode = StatusCodes.Status401Unauthorized;
                    return Unauthorized(response);
                }

                model.CreatedBy = Convert.ToInt32(Userid);

                var result = await _IItemMasterBL.AddCategoryMaster(model);

                if (result.IsSuccess == false)
                {
                    response.message = "Failed to add category";
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


        [HttpPost("GetCategoryMaster")]
        public async Task<IActionResult> GetCategoryMaster([FromBody] FilterModelTO filter)
        {
            var response = new GlobalResponseModel<List<TblCategoryMasterTO>>
            {
                message = "Category fetched successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (filter == null)
            {
                response.message = "Invalid filter data";
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
                var result = await _IItemMasterBL.GetListOfCategoryMaster(filter);

                if (result == null || result.List == null || result.List.Count == 0)
                {
                    response.message = "No categories found";
                    response.status = "Fail";
                    response.statusCode = StatusCodes.Status404NotFound;
                    response.data = null;
                    return Ok(response);
                }
                response.TotalRecords = result.TotalCount ?? 0;
                response.data = result.List.ToList();
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

        [HttpPost("UpdateCategoryMaster")]
        public async Task<IActionResult> UpdateCategoryMaster([FromBody] TblCategoryMasterTO model)
        {
            var response = new GlobalResponseModel<ResultMessage>
            {
                message = "Category updated successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (model == null || model.IdCategory <= 0 || string.IsNullOrWhiteSpace(model.CategoryName))
            {
                response.message = "Invalid category data";
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

                model.UpdatedBy = Convert.ToInt32(userId);
                model.UpdatedAt = DateTime.Now;

                var result = await _IItemMasterBL.UpdateCategoryMaster(model);

                if (result.IsSuccess == false)
                {
                    response.message = "Failed to update category";
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



        [HttpGet("GetCategoryById")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var response = new GlobalResponseModel<TblCategoryMasterTO>
            {
                message = "Category fetched successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (id <= 0)
            {
                response.message = "Invalid category id";
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

                var result = await _IItemMasterBL.GetCategoryById(id);

                if (result == null)
                {
                    response.message = "Category not found";
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
        #endregion

        #region Item Master 
        [HttpPost("AddItemMaster")]
        public async Task<IActionResult> AddItemMaster([FromBody] TblItemMasterTO model)
        {
            var response = new GlobalResponseModel<ResultMessage>
            {
                message = "Item added successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (model == null || string.IsNullOrWhiteSpace(model.ItemName))
            {
                response.message = "Invalid item data";
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

                model.CreatedBy = Convert.ToInt32(userId);

                var result = await _IItemMasterBL.AddItemMaster(model);

                if (result.IsSuccess == false)
                {
                    response.message = "Failed to add item";
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

        [HttpPost("GetItemMaster")]
        public async Task<IActionResult> GetItemMaster([FromBody] FilterModelTO filter)
        {
            var response = new GlobalResponseModel<List<TblItemMasterTO>>
            {
                message = "Items fetched successfully",
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

                var result = await _IItemMasterBL.GetListOfItemMaster(filter);
                response.data = result.List.ToList() ?? new List<TblItemMasterTO>();
                response.TotalRecords = result.TotalCount;
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

        [HttpGet("GetItemMasterById")]
        public async Task<IActionResult> GetItemMasterById(int id)
        {
            var response = new GlobalResponseModel<TblItemMasterTO>
            {
                message = "Item fetched successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (id <= 0)
            {
                response.message = "Invalid item id";
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

                var result = await _IItemMasterBL.GetItemMasterById(id);

                if (result == null)
                {
                    response.message = "Item not found";
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

        [HttpPost("UpdateItemMaster")]
        public async Task<IActionResult> UpdateItemMaster([FromBody] TblItemMasterTO model)
        {
            var response = new GlobalResponseModel<ResultMessage>
            {
                message = "Item updated successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (model == null || model.IdItemMaster <= 0)
            {
                response.message = "Invalid item data";
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

                model.UpdatedBy = Convert.ToInt32(userId);
                model.UpdatedAt = DateTime.Now;

                var result = await _IItemMasterBL.UpdateItemMaster(model);

                if (result.IsSuccess == false)
                {
                    response.message = "Failed to update item";
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
        #endregion
    }
}

