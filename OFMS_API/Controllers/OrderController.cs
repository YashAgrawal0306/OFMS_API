using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OFMS_API.BL.Interface;
using OFMS_API.Models;
using OFMS_API.Models.DTO;

namespace OFMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderBL _iOrderbl;
        public OrderController(IOrderBL iOrderbl) => _iOrderbl = iOrderbl;

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
            // Early return for validation
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
                // Assuming a method GetMyCartItems exists in the business layer
                var cartItems = await _iOrderbl.GetMyCartItem(userId).ConfigureAwait(false);
                if (cartItems == null)
                {
                    response.message = "No cart items found";
                    response.statusCode = StatusCodes.Status404NotFound;
                    response.status = "Fail";
                    response.data = new List<MyCartDTO>();
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
                response.data = null;
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

    }
}