using DTO.Models.CommonModel;
using DTO.Models.Master.OrderMaster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OFMS_API.Models;
using Services.BL.Interface.Master.CookAssignment;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OFMS_API.Controllers.Master.CookAssign
{
    [Route("api/[controller]")]
    [ApiController]
    public class CookAssignController : ControllerBase
    {
        private readonly ICookAssignBL _bl;

        public CookAssignController(ICookAssignBL bl)
        {
            _bl = bl;
        }

        [HttpPost("CreateCookAssignment")]
        public async Task<IActionResult> CreateCookAssignment([FromBody] List<CreateCookAssignmentTO> models)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "Cook Assignments added successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (models == null || models.Count == 0)
            {
                response.message = "Invalid assignment data";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = 0;
                return BadRequest(response);
            }

            try
            {
                int result = await _bl.CreateCookAssignmentBL(models);
                if (result <= 0)
                {
                    response.message = "Failed to add cook assignment";
                    response.statusCode = StatusCodes.Status500InternalServerError;
                    response.status = "Error";
                    response.data = result;
                    return Ok(response);
                }

                response.data = result;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.exception = ex;
                response.message = Helper.Common.Utility.FormatExceptionMessage(ex);
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status = "Error";
                response.data = 0;
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpPut("UpdateKitchenStatus")]
        public async Task<IActionResult> UpdateKitchenStatus([FromBody] UpdateKitchenStatusTO model)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "Kitchen Status updated successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (model == null || model.IdCookAssignment <= 0)
            {
                response.message = "Invalid update data";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = 0;
                return BadRequest(response);
            }

            try
            {
                int result = await _bl.UpdateKitchenStatusBL(model);

                if (result <= 0)
                {
                    response.message = "Kitchen Status update failed";
                    response.statusCode = StatusCodes.Status500InternalServerError;
                    response.status = "Error";
                    response.data = result;
                    return Ok(response);
                }

                response.data = result;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.exception = ex;
                response.message = Helper.Common.Utility.FormatExceptionMessage(ex);
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status = "Error";
                response.data = 0;
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpPost("GetCookAssignmentList")]
        public async Task<IActionResult> GetCookAssignmentList([FromBody] FilterModelTO filterModelTO)
        {
            var response = new GlobalResponseModel<List<CookAssignmentResponseTO>>
            {
                message = "Cook Assignment retrieved successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            try
            {
                var result = await _bl.GetCookAssignmentListBL(filterModelTO);

                if (result == null || result.Count == 0)
                {
                    response.message = "No assignments found";
                    response.statusCode = StatusCodes.Status404NotFound;
                    response.status = "Fail";
                    response.data = null;
                    return Ok(response);
                }

                response.data = result;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.exception = ex;
                response.message = Helper.Common.Utility.FormatExceptionMessage(ex);
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status = "Error";
                response.data = null;
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpGet("GetMergeableItems")]
        public async Task<IActionResult> GetMergeableItems()
        {
            var response = new GlobalResponseModel<List<MergeableItemResponseTO>>
            {
                message = "Mergeable items retrieved successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            try
            {
                var result = await _bl.GetMergeableCookItemsBL();
                response.data = result;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.exception = ex;
                response.message = Helper.Common.Utility.FormatExceptionMessage(ex);
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status = "Error";
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpPost("AssignMergedCookItem")]
        public async Task<IActionResult> AssignMergedCookItem([FromBody] MergedCookAssignmentRequestTO model)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "Merged Cook Assignment created successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (model == null || model.SourceOrders == null || model.SourceOrders.Count == 0)
            {
                response.message = "Invalid assignment data";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                return BadRequest(response);
            }

            try
            {
                int result = await _bl.AssignMergedCookItemBL(model);
                if (result <= 0)
                {
                    response.message = "Failed to add merged cook assignment";
                    response.statusCode = StatusCodes.Status500InternalServerError;
                    response.status = "Error";
                    return Ok(response);
                }

                response.data = result;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.exception = ex;
                response.message = Helper.Common.Utility.FormatExceptionMessage(ex);
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status = "Error";
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpPut("UpdateMergedKitchenStatus")]
        public async Task<IActionResult> UpdateMergedKitchenStatus([FromBody] UpdateKitchenStatusTO model)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "Merged Kitchen Status updated successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (model == null || model.IdCookAssignment <= 0)
            {
                response.message = "Invalid update data";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                return BadRequest(response);
            }

            try
            {
                int result = await _bl.UpdateMergedKitchenStatusBL(model);
                response.data = result;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.exception = ex;
                response.message = Helper.Common.Utility.FormatExceptionMessage(ex);
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status = "Error";
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
    }
}
