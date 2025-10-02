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
        #region GetAllUserList

        /// <summary>
        /// Retrieves a filtered list of users
        /// </summary>
        /// <param name="filter">FilterModelTO object containing filter criteria</param>
        /// <returns>GlobalResponseModel with list of users</returns>
        /// <response code="200">Users retrieved successfully</response>
        /// <response code="204">No users found</response>
        /// <response code="500">Server error</response>
        [HttpPost("GetAllUserInfo")]
        public async Task<IActionResult> GetAllUserList([FromBody] FilterModelTO filter)
        {
            var response = new GlobalResponseModel<OutPutClass<TblUserTO>>
            {
                message = "Users retrieved successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            try
            {
                var result = await db.GetAllCust(filter).ConfigureAwait(false);

                if (result == null)
                {
                    response.message = "No users found";
                    response.statusCode = StatusCodes.Status204NoContent;
                    response.status = "Success";
                    response.data = new OutPutClass<TblUserTO>();
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
                response.data = new OutPutClass<TblUserTO>();
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        #endregion

        #region AddNewUserTO
        [HttpPost("AddNewUser")]
        [AllowAnonymous]
        public async Task<IActionResult> AddNewUserTO([FromForm] TblUserTO user)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "User added successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (user == null || string.IsNullOrWhiteSpace(user.UserName))
            {
                response.message = "Invalid user data";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = 0;
                return BadRequest(response);
            }

            try
            {
                int result = await db.AddNewCustomerBL(user);
                response.data = result;

                if (result != 1)
                {
                    response.message = "Data not added";
                    response.status = "Error";
                    response.statusCode = StatusCodes.Status500InternalServerError;
                }

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

        #region LoginUser

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginUser([FromBody] TblUserLogin login)
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

        #region Member Management

        /// <summary>
        /// Retrieves a filtered list of members
        /// </summary>
        /// <param name="filter">FilterModelTO object containing filter criteria</param>
        /// <returns>GlobalResponseModel with list of members</returns>
        /// <response code="200">Members retrieved successfully</response>
        /// <response code="204">No members found</response>
        /// <response code="500">Server error</response>
        [HttpPost("GetAllMemberList")]
        [ProducesResponseType(typeof(GlobalResponseModel<List<TblUserTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GlobalResponseModel<object[]>), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(GlobalResponseModel<object[]>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllMemberList([FromBody] FilterModelTO filter)
        {
            var response = new GlobalResponseModel<OutPutClass<TblUserTO>>
            {
                message = "Members retrieved successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            try
            {
                var memberList = await db.GetAllMemberList(filter).ConfigureAwait(false);

                if (memberList == null)
                {
                    response.message = "No members found";
                    response.statusCode = StatusCodes.Status204NoContent;
                    response.status = "Success";
                    response.data = new OutPutClass<TblUserTO>();
                    return Ok(response);
                }

                response.data = memberList;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.exception = ex;
                response.message = Helper.Common.Utility.FormatExceptionMessage(ex);
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status = "Error";
                response.data = new OutPutClass<TblUserTO>();
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        #endregion

    }
}
