using DTO.Models.CommonModel;
using DTO.Models.Master.ItemMaster;
using DTO.Models.Master.ItemMaster.ResponseModel;
using DTO.Models.Master.OrderMaster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OFMS_API.BL.Interface;

namespace OFMS_API.Controllers.Master.OrderMaster
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderMasterController : ControllerBase
    {
        private readonly IOrderBL _iOrderBL;
        public OrderMasterController(IOrderBL iOrderBL)
        {
            _iOrderBL = iOrderBL;
        }
        [HttpPost("AddOrderMaster")]
        public async Task<IActionResult> AddOrderMaster([FromBody] OrderMasterTO model)
        {
            var response = new GlobalResponseModel<ResultMessage>
            {
                message = "Order added successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (model == null)
            {
                response.message = "Invalid order data";
                response.status = "Fail";
                response.statusCode = StatusCodes.Status400BadRequest;
                return BadRequest(response);
            }
            try
            {
                var userIdClaim = User.FindFirst("userId");
                int? Userid = userIdClaim != null ? int.Parse(userIdClaim.Value) : null;
                if (Userid == null || Userid == 0)
                {
                    response.message = "Unauthorized user";
                    response.status = "Fail";
                    response.statusCode = StatusCodes.Status401Unauthorized;
                    return Unauthorized(response);
                }

                // Auto-generate OrderNo if missing
                if (string.IsNullOrWhiteSpace(model.OrderNo))
                {
                    model.OrderNo = "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(1000, 9999);
                }

                // Default CustomerId to logged in user if not provided
                if (model.CustomerId <= 0)
                {
                    model.CustomerId = Convert.ToInt32(Userid);
                }

                // Auto-manage Order Status: default to New (1)
                if (model.IdStatus <= 0)
                {
                    model.IdStatus = 1;
                }

                // Initialize default PaymentDetail to prevent NullReferenceException in OrderBL
                if (model.PaymentDetail == null)
                {
                    model.PaymentDetail = new TblPaymentTO
                    {
                        Amount = model.GrandTotal,
                        PaymentMethod = "Cash",
                        TransactionNo = "COD-" + model.OrderNo,
                        TransactionTypeId = "1",
                        IdStatus = 1,
                        IsActive = true,
                        CreatedOn = DateTime.Now,
                        CreatedBy = Convert.ToInt32(Userid)
                    };
                }
                else
                {
                    if (model.PaymentDetail.Amount == 0) model.PaymentDetail.Amount = model.GrandTotal;
                    if (string.IsNullOrWhiteSpace(model.PaymentDetail.PaymentMethod)) model.PaymentDetail.PaymentMethod = "Cash";
                    if (string.IsNullOrWhiteSpace(model.PaymentDetail.TransactionNo)) model.PaymentDetail.TransactionNo = "COD-" + model.OrderNo;
                    if (string.IsNullOrWhiteSpace(model.PaymentDetail.TransactionTypeId)) model.PaymentDetail.TransactionTypeId = "1";
                    if (model.PaymentDetail.IdStatus == 0) model.PaymentDetail.IdStatus = 1;
                    model.PaymentDetail.IsActive = true;
                    model.PaymentDetail.CreatedBy = Convert.ToInt32(Userid);
                }

                model.CreatedBy = Convert.ToInt32(Userid);
                var result = await _iOrderBL.AddOrderMaster(model);

                if (result.IsSuccess == false)
                {
                    response.message = "Failed to add Order";
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
        [HttpPost("GetAllOrderMasterList")]
        public async Task<IActionResult> GetOrderMasterList(OrderListFilter orderListFilter)
            {
            var response = new GlobalResponseModel<List<OrderListResponseTO>>
            {
                message = "Groups fetched successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            try
            {
                var data = await _iOrderBL.GetOrderMasterList(orderListFilter);
                response.data = data.List;
                response.TotalRecords = data.TotalCount;


                return Ok(response);
            }
            catch (Exception ex)
            {
                response.message = ex.Message;
                response.exception = ex;
                response.status = "Error addd";
                response.statusCode = StatusCodes.Status500InternalServerError;

                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpGet("GetOrderMasterListByIdOrder")]
        public async Task<IActionResult> GetOrderMasterListByIdOrder(int IdOrderMaster)
        {
            var response = new GlobalResponseModel<OrderListResponseTO>
            {
                message = "Groups fetched successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            try
            {
                var data = await _iOrderBL.GetOrderMasterListByIdOrder(IdOrderMaster);
                response.data = data; 
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.message = ex.Message;
                response.exception = ex;
                response.status = "Error addd";
                response.statusCode = StatusCodes.Status500InternalServerError;

                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpPost("UpdateOrderMaster")]
        public async Task<IActionResult> UpdateOrderMaster(OrderMasterTO order)
        {
            try
            {
                bool result = await _iOrderBL.UpdateOrderMaster(order);

                return Ok(new
                {
                    Status = "Success",
                    Message = "Order updated successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = ex.Message
                });
            }
        }

        [HttpPost("UpdateOrderStatus")]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusRequest request)
        {
            var response = new GlobalResponseModel<ResultMessage>();
            try
            {
                var result = await _iOrderBL.UpdateOrderStatus(request);
                response.statusCode = StatusCodes.Status200OK;
                response.status = "Success";
                response.message = result.Message;
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

        [HttpGet("GenerateInvoice/{orderId}")]
        public async Task<IActionResult> GenerateInvoice(int orderId)
        {
            try
            {
                var pdfBytes = await _iOrderBL.GenerateOrderInvoiceAsync(orderId);
                return File(pdfBytes, "application/pdf", $"Invoice_{orderId}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Status = "Error", Message = ex.Message });
            }
        }
    }
}
