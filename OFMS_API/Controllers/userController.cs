using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OFMS_API.BL.Interface;
using OFMS_API.DAL.Interface;
using OFMS_API.Models;

namespace OFMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class userController : ControllerBase
    {
        private readonly IuserBL db;
        public userController(IuserBL add)
        {
            db = add;
        }
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(db.GetAllCust());
        }

        #region User Management
        [HttpPost("AddNewUser")]
        [AllowAnonymous]
        public async Task<IActionResult> AddNewUserTO([FromBody] object data)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "User added successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            // Early return for null or invalid payload
            if (data == null)
            {
                response.message = "Invalid user data";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = 0;
                return BadRequest(response);
            }

            try
            {
                string json = data.ToString();
                tbluser user = JsonConvert.DeserializeObject<tbluser>(json);

                if (user == null || string.IsNullOrWhiteSpace(user.UserName))
                {
                    response.message = "User deserialization failed or missing required fields";
                    response.statusCode = StatusCodes.Status400BadRequest;
                    response.status = "Fail";
                    response.data = 0;
                    return BadRequest(response);
                }

                int result = await db.AddNewCustomerBL(user).ConfigureAwait(false);

                if (result != 1)
                {
                    response.message = "Data is not added";
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

        #endregion

        #region User Authentication

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginUser([FromBody] tbluserlogin login)
        {
            var response = new GlobalResponseModel<string>
            {
                message = "Login successful",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            // Early return for validation
            if (login == null || string.IsNullOrWhiteSpace(login.Email) || string.IsNullOrWhiteSpace(login.Password))
            {
                response.message = "Invalid login credentials";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = string.Empty;
                return BadRequest(response);
            }

            try
            {
                string result = await db.LoginBL(login).ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(result))
                {
                    response.message = "Login failed";
                    response.statusCode = StatusCodes.Status500InternalServerError;
                    response.status = "Error";
                    response.data = string.Empty;
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
                response.data = string.Empty;
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        #endregion

    }
}
