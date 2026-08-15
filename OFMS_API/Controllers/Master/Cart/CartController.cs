using DTO.Models.CommonModel;
using DTO.Models.Master.Cart;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OFMS_API.BL.Interface;

namespace OFMS_API.Controllers.Master.Cart
{
    /// <summary>
    /// Cart endpoints for the Customer-facing screen.
    /// CustomerId is ALWAYS extracted from the JWT token — never from the request body.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartBL _cartBL;

        public CartController(ICartBL cartBL)
        {
            _cartBL = cartBL;
        }

        // ── Helper: extract authenticated user's ID ────────────────
        private int? GetCustomerId()
        {
            var claim = User.FindFirst("userId");
            if (claim == null) return null;
            return int.TryParse(claim.Value, out int id) ? id : null;
        }

        // ── POST /api/Cart/add ─────────────────────────────────────
        /// <summary>Add item to cart. Increases quantity if already present.</summary>
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddCartRequest request)
        {
            var response = new GlobalResponseModel<string>();

            var customerId = GetCustomerId();
            if (customerId is null or 0)
            {
                response.message   = "Unauthorized user.";
                response.status    = "Fail";
                response.statusCode = StatusCodes.Status401Unauthorized;
                return Unauthorized(response);
            }

            if (request == null || request.IdItemMaster <= 0)
            {
                response.message   = "Invalid request. IdItemMaster is required.";
                response.status    = "Fail";
                response.statusCode = StatusCodes.Status400BadRequest;
                return BadRequest(response);
            }

            try
            {
                var (success, message) = await _cartBL.AddToCartAsync(request, customerId.Value);
                if (!success)
                {
                    response.message   = message;
                    response.status    = "Fail";
                    response.statusCode = StatusCodes.Status400BadRequest;
                    return BadRequest(response);
                }

                response.message   = message;
                response.status    = "Success";
                response.statusCode = StatusCodes.Status200OK;
                response.data      = message;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.message   = ex.Message;
                response.status    = "Error";
                response.statusCode = StatusCodes.Status500InternalServerError;
                // response.exception = ex;
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        // ── GET /api/Cart ─────────────────────────────────────────
        /// <summary>Return the logged-in customer's complete cart with totals.</summary>
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var response = new GlobalResponseModel<CartSummaryResponse>();

            var customerId = GetCustomerId();
            if (customerId is null or 0)
            {
                response.message   = "Unauthorized user.";
                response.status    = "Fail";
                response.statusCode = StatusCodes.Status401Unauthorized;
                return Unauthorized(response);
            }

            try
            {
                var summary = await _cartBL.GetCartAsync(customerId.Value);
                response.message   = "Cart retrieved successfully.";
                response.status    = "Success";
                response.statusCode = StatusCodes.Status200OK;
                response.data      = summary;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.message   = ex.Message;
                response.status    = "Error";
                response.statusCode = StatusCodes.Status500InternalServerError;
                // response.exception = ex;
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        // ── PUT /api/Cart/update ──────────────────────────────────
        /// <summary>Update item quantity. Quantity 0 removes the item.</summary>
        [HttpPut("update")]
        public async Task<IActionResult> UpdateCart([FromBody] UpdateCartRequest request)
        {
            var response = new GlobalResponseModel<string>();

            var customerId = GetCustomerId();
            if (customerId is null or 0)
            {
                response.message   = "Unauthorized user.";
                response.status    = "Fail";
                response.statusCode = StatusCodes.Status401Unauthorized;
                return Unauthorized(response);
            }

            if (request == null || request.IdCart <= 0)
            {
                response.message   = "Invalid request. IdCart is required.";
                response.status    = "Fail";
                response.statusCode = StatusCodes.Status400BadRequest;
                return BadRequest(response);
            }

            try
            {
                var (success, message) = await _cartBL.UpdateCartAsync(request, customerId.Value);
                if (!success)
                {
                    response.message   = message;
                    response.status    = "Fail";
                    response.statusCode = StatusCodes.Status400BadRequest;
                    return BadRequest(response);
                }

                response.message   = message;
                response.status    = "Success";
                response.statusCode = StatusCodes.Status200OK;
                response.data      = message;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.message   = ex.Message;
                response.status    = "Error";
                response.statusCode = StatusCodes.Status500InternalServerError;
                // response.exception = ex;
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        // ── DELETE /api/Cart/remove/{idCart} ─────────────────────
        /// <summary>Remove a specific cart item.</summary>
        [HttpDelete("remove/{idCart:int}")]
        public async Task<IActionResult> RemoveCartItem(int idCart)
        {
            var response = new GlobalResponseModel<string>();

            var customerId = GetCustomerId();
            if (customerId is null or 0)
            {
                response.message   = "Unauthorized user.";
                response.status    = "Fail";
                response.statusCode = StatusCodes.Status401Unauthorized;
                return Unauthorized(response);
            }

            try
            {
                var (success, message) = await _cartBL.RemoveCartItemAsync(idCart, customerId.Value);
                response.message   = message;
                response.status    = success ? "Success" : "Fail";
                response.statusCode = StatusCodes.Status200OK;
                response.data      = message;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.message   = ex.Message;
                response.status    = "Error";
                response.statusCode = StatusCodes.Status500InternalServerError;
                // response.exception = ex;
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        // ── DELETE /api/Cart/clear ────────────────────────────────
        /// <summary>Clear all items from the logged-in customer's cart.</summary>
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var response = new GlobalResponseModel<string>();

            var customerId = GetCustomerId();
            if (customerId is null or 0)
            {
                response.message   = "Unauthorized user.";
                response.status    = "Fail";
                response.statusCode = StatusCodes.Status401Unauthorized;
                return Unauthorized(response);
            }

            try
            {
                var (success, message) = await _cartBL.ClearCartAsync(customerId.Value);
                response.message   = message;
                response.status    = success ? "Success" : "Fail";
                response.statusCode = StatusCodes.Status200OK;
                response.data      = message;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.message   = ex.Message;
                response.status    = "Error";
                response.statusCode = StatusCodes.Status500InternalServerError;
                // response.exception = ex;
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        // ── GET /api/Cart/count ───────────────────────────────────
        /// <summary>Return the number of active cart items (for the badge).</summary>
        [HttpGet("count")]
        public async Task<IActionResult> GetCartCount()
        {
            var response = new GlobalResponseModel<int>();

            var customerId = GetCustomerId();
            if (customerId is null or 0)
            {
                // Return 0 silently when not logged in — badge should show nothing
                response.data      = 0;
                response.message   = "Not authenticated.";
                response.status    = "Success";
                response.statusCode = StatusCodes.Status200OK;
                return Ok(response);
            }

            try
            {
                int count = await _cartBL.GetCartCountAsync(customerId.Value);
                response.message   = "Cart count retrieved.";
                response.status    = "Success";
                response.statusCode = StatusCodes.Status200OK;
                response.data      = count;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.message   = ex.Message;
                response.status    = "Error";
                response.statusCode = StatusCodes.Status500InternalServerError;
                // response.exception = ex;
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
    }
}
