using Microsoft.AspNetCore.Mvc;
using OFMS_API.BL.Interface;
using OFMS_API.Models;
using OFMS_API.Models.DTO;

namespace OFMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController(IOrderBL _iOrderbl): ControllerBase
    {
        //private readonly IOrderBL _iOrderbl;
        //public OrderController(IOrderBL iOrderbl) => _iOrderbl = iOrderbl;

        #region Cart Management
        [HttpPost("AddTOCart")]
        public async Task<IActionResult> AddTOCart([FromBody] CartTO cart)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "Item added to cart successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };
            int user = Convert.ToInt32(User.FindFirst("userId")?.Value);
            cart.UserId = user;
            if (cart == null || cart.MenuItemId <= 0 || cart.Quantity <= 0)
            {
                response.message = "Invalid cart data";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = 0;
                return BadRequest(response);
            }

            try
            {
                int id = await _iOrderbl.AddToCart(cart).ConfigureAwait(false);

                if (id <= 0)
                {
                    response.message = "This Item is Already Exist";
                    response.statusCode = StatusCodes.Status500InternalServerError;
                    response.status = "Error";
                    response.data = id;
                    return BadRequest(response);
                }
                response.data = id;
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
        #endregion

        #region GetMyCartItem

        [HttpGet("GetMyCartItem")]
        public async Task<IActionResult> GetMyCartItem()
        {
            var response = new GlobalResponseModel<List<MyCartDTO>>
            {
                message = "Cart items retrieved successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };
            int userId = Convert.ToInt32(User.FindFirst("userId")?.Value);
            try
            {
                var cartItems = await _iOrderbl.GetMyCartItem(userId).ConfigureAwait(false);
                if (cartItems == null)
                {
                    response.message = "No cart items found";
                    response.statusCode = StatusCodes.Status404NotFound;
                    response.status = "Fail";
                    response.data = [];
                    return Ok(response);
                }
                response.data = cartItems;
                return Ok(response);
            }
            catch (Exception ex)
            {
              
                response.exception = ex;
                response.message = Helper.Common.Utility.FormatExceptionMessage(ex);
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status = "Error";
                response.data = [];
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        #endregion

        #region RemoveCartItem
        #region Cart Management
 
        [HttpDelete("RemoveCartItem")] 
        public async Task<IActionResult> RemoveCartItem(int cartid)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "Cart item removed successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };
             
            if (cartid <= 0)
            {
                response.message = "Invalid cart ID";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = 0;
                return BadRequest(response);
            }

            try
            {
                int result = await _iOrderbl.RemoveCartItem(cartid).ConfigureAwait(false);

                if (result <= 0)
                {
                    response.message = "Cart item not found or could not be removed";
                    response.statusCode = StatusCodes.Status404NotFound;
                    response.status = "Fail";
                    response.data = result;
                    return NotFound(response);
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

        #endregion

        #endregion


        #region Cart Management
        [HttpGet("GetCartSummaryDeatil")]
       
        public async Task<IActionResult> GetCartSummaryDeatil()
        {
            var response = new GlobalResponseModel<CartSummaryDetails>
            {
                message = "Cart summary retrieved successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            try
            { 
                var userIdClaim = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId) || userId <= 0)
                {
                    response.message = "Unauthorized or invalid user";
                    response.statusCode = StatusCodes.Status401Unauthorized;
                    response.status = "Fail";
                    response.data = null!;
                    return Unauthorized(response);
                }

                var result = await _iOrderbl.GetCartSummaryDeatil(userId).ConfigureAwait(false);

                if (result == null)
                {
                    response.message = "No cart items found";
                    response.statusCode = StatusCodes.Status204NoContent;
                    response.status = "Success";
                    response.data = null!;
                    return StatusCode(StatusCodes.Status204NoContent, response);
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
                response.data = null!;
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        #endregion 
        #region Cart Management
        [HttpPut("UpdateCartItem")] 
        public async Task<IActionResult> UpdateCartItem(int cartId, int quantity)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "Cart item updated successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };
             
            if (cartId <= 0 || quantity <= 0)
            {
                response.message = "Invalid cart ID or quantity";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = 0;
                return BadRequest(response);
            }

            try
            {
                int result = await _iOrderbl.UpdateCartItem(cartId, quantity).ConfigureAwait(false);

                if (result <= 0)
                {
                    response.message = "Cart item not found or update failed";
                    response.statusCode = StatusCodes.Status404NotFound;
                    response.status = "Fail";
                    response.data = result;
                    return NotFound(response);
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
        #endregion

    }
}