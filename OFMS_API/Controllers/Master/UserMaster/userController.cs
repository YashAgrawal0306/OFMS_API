using DTO.Models.CommonModel;
using DTO.Models.Master.UserMaster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OFMS_API.BL.Interface;

namespace OFMS_API.Controllers.Master.UserMaster
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IuserBL db) : ControllerBase
    {
        //private readonly IuserBL db;

        //public UserController(IuserBL add) => db = add;

        #region GetAllUserList
        [HttpPost("GetAllUserInfo")]
        public async Task<IActionResult> GetAllUserList([FromBody] FilterModelTO filter)
        {
            var response = new GlobalResponseModel<OutPutClass<TblUserResponseTO>>
            {
                message = "Users retrieved successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            try
            {
                var result = await db.GetAllCust(filter);

                if (result == null)
                {
                    response.message = "No users found";
                    response.statusCode = StatusCodes.Status204NoContent;
                    response.status = "Success";
                    response.data = new OutPutClass<TblUserResponseTO>();
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

        [HttpGet("GetUserByIdUser")]
        public async Task<IActionResult> GetUserByIdUser(int idUser)
        {
            var response = new GlobalResponseModel<TblUserResponseTO>
            {
                message = "Users retrieved successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            try
            {
                var result = await db.GetUserByIdUser(idUser);

                if (result == null)
                {
                    response.message = "No users found";
                    response.statusCode = StatusCodes.Status204NoContent;
                    response.status = "Success";
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

                if (result <= 0)
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

        /// <summary>
        /// Creates a new user (Member or Customer) with an optional address in one atomic transaction.
        /// Works for any role. EntityType is derived automatically from RoleId.
        /// Lat/Long are handled server-side (not required from frontend).
        /// </summary>
        [HttpPost("AddNewUserWithAddress")]
        [AllowAnonymous]
        public async Task<IActionResult> AddNewUserWithAddress([FromForm] TblUserWithAddressTO model)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "User added successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (model == null || string.IsNullOrWhiteSpace(model.UserName))
            {
                response.message = "Invalid user data";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = 0;
                return BadRequest(response);
            }

            try
            {
                int userId = await db.AddNewUserWithAddressBL(model);
                response.data = userId;

                if (userId <= 0)
                {
                    response.message = "Failed to add user";
                    response.status = "Error";
                    response.statusCode = StatusCodes.Status500InternalServerError;
                    return Ok(response);
                }

                response.message = "User added successfully";
                response.status = "Success";
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

        [HttpPost("EditUser")]
        public async Task<IActionResult> EditUserTO([FromForm] TblUserTO user)
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
                int result = await db.EditUserTO(user);
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

        [HttpPost("GetAllMemberList")]
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

        #region Role 

        [HttpGet("GetAllRoles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var response = new GlobalResponseModel<List<TblRoleTO>>
            {
                message = "Roles retrieved successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            try
            {
                var roles = await db.GetAllRoles();

                if (roles == null || roles.Count == 0)
                {
                    response.message = "No roles found";
                    response.statusCode = StatusCodes.Status204NoContent;
                    response.status = "Success";
                    response.data = new List<TblRoleTO>();
                    return Ok(response);
                }

                response.data = roles;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.exception = ex;
                response.message = Helper.Common.Utility.FormatExceptionMessage(ex);
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status = "Error";
                response.data = new List<TblRoleTO>();
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        #endregion
    }
}
