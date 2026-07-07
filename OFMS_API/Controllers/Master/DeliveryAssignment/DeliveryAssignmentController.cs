using DTO.Models.CommonModel;
using DTO.Models.Master.OrderMaster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.BL.Interface.Master.DeliveryAssignment;
using System;
using System.Threading.Tasks;

namespace OFMS_API.Controllers.Master.DeliveryAssignment
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveryAssignmentController : ControllerBase
    {
        private readonly IDeliveryAssignmentBL _bl;

        public DeliveryAssignmentController(IDeliveryAssignmentBL bl)
        {
            _bl = bl;
        }

        [HttpPost("AssignDeliveryBoy")]
        public async Task<IActionResult> AssignDeliveryBoy([FromBody] CreateDeliveryAssignmentTO model)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "Delivery status updated successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (model == null)
            {
                response.message = "Invalid payload";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Failed";
                return BadRequest(response);
            }

            try
            {
                int result = await _bl.AssignDeliveryBoy(model);
                if (result <= 0)
                {
                    response.message = "Failed to assign delivery boy";
                    response.statusCode = StatusCodes.Status400BadRequest;
                    response.status = "Failed";
                }
                else
                {
                    response.data = result;
                    response.message = "Delivery boy assigned successfully";
                }
            }
            catch (Exception ex)
            {
                response.message = ex.Message;
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status = "Failed";
            }

            return Ok(response);
        }

        [HttpGet("GetDeliveryAssignmentById/{id}")]
        public async Task<IActionResult> GetDeliveryAssignmentById(int id)
        {
            var response = new GlobalResponseModel<DeliveryAssignmentResponseTO>
            {
                message = "Delivery assignment retrieved successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            try
            {
                var result = await _bl.GetDeliveryAssignmentById(id);
                if (result == null)
                {
                    response.message = "Delivery assignment not found";
                    response.statusCode = StatusCodes.Status404NotFound;
                    response.status = "Failed";
                }
                else
                {
                    response.data = result;
                }
            }
            catch (Exception ex)
            {
                response.message = ex.Message;
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status = "Failed";
            }

            return Ok(response);
        }

        [HttpPost("GetAllDeliveryAssignments")]
        public async Task<IActionResult> GetAllDeliveryAssignments([FromBody] FilterModelTO filter)
        {
            var response = new GlobalResponseModel<OutPutClass<DeliveryAssignmentResponseTO>>
            {
                message = "Delivery assignments retrieved successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            try
            {
                var result = await _bl.GetAllDeliveryAssignments(filter);
                response.data = result;
            }
            catch (Exception ex)
            {
                response.message = ex.Message;
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status = "Failed";
            }

            return Ok(response);
        }

        [HttpPost("AcceptDelivery")]
        public async Task<IActionResult> AcceptDelivery([FromBody] ActionDeliveryTO model)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "Delivery status updated successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            try
            {
                int result = await _bl.AcceptDelivery(model.IdDeliveryAssignment, model.UpdatedBy);
                if (result <= 0)
                {
                    response.message = "Failed to update status";
                    response.statusCode = StatusCodes.Status400BadRequest;
                    response.status = "Failed";
                }
                else
                {
                    response.data = result;
                }
            }
            catch (Exception ex)
            {
                response.message = ex.Message;
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status = "Failed";
            }

            return Ok(response);
        }

        [HttpPost("PickUpOrder")]
        public async Task<IActionResult> PickUpOrder([FromBody] ActionDeliveryTO model)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "Delivery status updated successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            try
            {
                int result = await _bl.PickUpOrder(model.IdDeliveryAssignment, model.UpdatedBy);
                if (result <= 0)
                {
                    response.message = "Failed to update status";
                    response.statusCode = StatusCodes.Status400BadRequest;
                    response.status = "Failed";
                }
                else
                {
                    response.data = result;
                }
            }
            catch (Exception ex)
            {
                response.message = ex.Message;
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status = "Failed";
            }

            return Ok(response);
        }

        [HttpPost("MarkDelivered")]
        public async Task<IActionResult> MarkDelivered([FromBody] ActionDeliveryTO model)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "Delivery status updated successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            try
            {
                int result = await _bl.MarkDelivered(model.IdDeliveryAssignment, model.UpdatedBy);
                if (result <= 0)
                {
                    response.message = "Failed to update status";
                    response.statusCode = StatusCodes.Status400BadRequest;
                    response.status = "Failed";
                }
                else
                {
                    response.data = result;
                }
            }
            catch (Exception ex)
            {
                response.message = ex.Message;
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status = "Failed";
            }

            return Ok(response);
        }
    }
}
