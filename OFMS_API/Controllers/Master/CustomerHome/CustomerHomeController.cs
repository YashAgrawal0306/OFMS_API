using DTO.Models.CommonModel;
using DTO.Models.Master.CustomerHome;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OFMS_API.BL.Interface.Master.CustomerHome;
using System;
using System.Threading.Tasks;

namespace OFMS_API.Controllers.Master.CustomerHome
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerHomeController(ICustomerHomeBL bl) : ControllerBase
    {
        [HttpGet("GetLandingData")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLandingData()
        {
            var response = new GlobalResponseModel<CustomerHomeDataTO>
            {
                message = "Home page statistics retrieved successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            try
            {
                var data = await bl.GetCustomerHomeData();
                response.data = data;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.message = ex.Message;
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status = "Error";
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
    }
}
