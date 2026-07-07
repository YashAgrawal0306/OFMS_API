using DTO.Models.CommonModel;
using DTO.Models.Master.AddressMaster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OFMS_API.Models;
using Services.BL.Interface.Master.AddressMaster;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OFMS_API.Controllers.Master.AddressMaster
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressMasterController : ControllerBase
    {
        private readonly IAddressMasterBL _bl;

        public AddressMasterController(IAddressMasterBL bl)
        {
            _bl = bl;
        }

        #region tblAddressTO
        [HttpGet("GetAllAddresses")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllAddresses()
        {
            var response = new GlobalResponseModel<List<tblAddressTO>>
            {
                message = "Addresses retrieved successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            try
            {
                var data = await _bl.GetAllAddressesBL();

                if (data == null || data.Count == 0)
                {
                    response.message = "No addresses found";
                    response.statusCode = StatusCodes.Status204NoContent;
                    response.status = "Success";
                    response.data = new List<tblAddressTO>();
                    return Ok(response);
                }

                response.data = data;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.exception = ex;
                response.message = Helper.Common.Utility.FormatExceptionMessage(ex);
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status = "Error";
                response.data = new List<tblAddressTO>();
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
        [HttpGet("GetAddressById")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAddressById(int id)
        {
            var response = new GlobalResponseModel<tblAddressTO>
            {
                message = "Address retrieved successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (id <= 0)
            {
                response.message = "Invalid address ID";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                return BadRequest(response);
            }

            try
            {
                var data = await _bl.GetAddressByIdBL(id);

                if (data == null)
                {
                    response.message = "Address not found";
                    response.statusCode = StatusCodes.Status404NotFound;
                    return Ok(response);
                }

                response.data = data;
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

        [HttpGet("GetAddressByIdUser")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAddressByIdUser(int idUser)
        {
            var response = new GlobalResponseModel<tblAddressResponseTO>
            {
                message = "Address retrieved successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (idUser <= 0)
            {
                response.message = "Invalid address ID";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                return BadRequest(response);
            }

            try
            {
                var data = await _bl.GetAddressByIdUser(idUser);

                if (data == null)
                {
                    response.message = "Address not found";
                    response.statusCode = StatusCodes.Status404NotFound;
                    return Ok(response);
                }

                response.data = data;
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

        [HttpPost("AddAddress")]
        public async Task<IActionResult> AddAddress([FromBody] tblAddressTO model)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "Address added successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (model == null)
            {
                response.message = "Invalid address data";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = 0;
                return BadRequest(response);
            }

            try
            {
                int result = await _bl.AddAddressBL(model);
                if (result <= 0)
                {
                    response.message = "Failed to add address";
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
        [HttpPut("UpdateAddress")]
        public async Task<IActionResult> UpdateAddress([FromBody] tblAddressTO model)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "Address updated successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (model == null)
            {
                response.message = "Invalid address data";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = 0;
                return BadRequest(response);
            }

            try
            {
                int result = await _bl.UpdateAddressBL(model);

                if (result <= 0)
                {
                    response.message = "Address update failed";
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
        [HttpDelete("DeleteAddress")]
        public async Task<IActionResult> DeleteAddress(int idAddress)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "Address deleted successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (idAddress <= 0)
            {
                response.message = "Invalid address ID";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = 0;
                return BadRequest(response);
            }

            try
            {
                int result = await _bl.DeleteAddressBL(idAddress);

                if (result <= 0)
                {
                    response.message = "Address deletion failed";
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

        #region dimAddressTypeTO
        [HttpGet("GetAllAddressTypes")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllAddressTypes()
        {
            var response = new GlobalResponseModel<List<dimAddressTypeTO>>
            {
                message = "AddressTypes retrieved successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            try
            {
                var data = await _bl.GetAllAddressTypesBL();

                if (data == null || data.Count == 0)
                {
                    response.message = "No addresstypes found";
                    response.statusCode = StatusCodes.Status204NoContent;
                    response.status = "Success";
                    response.data = new List<dimAddressTypeTO>();
                    return Ok(response);
                }

                response.data = data;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.exception = ex;
                response.message = Helper.Common.Utility.FormatExceptionMessage(ex);
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status = "Error";
                response.data = new List<dimAddressTypeTO>();
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
        [HttpGet("GetAddressTypeById")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAddressTypeById(int id)
        {
            var response = new GlobalResponseModel<dimAddressTypeTO>
            {
                message = "AddressType retrieved successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (id <= 0)
            {
                response.message = "Invalid addresstype ID";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                return BadRequest(response);
            }

            try
            {
                var data = await _bl.GetAddressTypeByIdBL(id);

                if (data == null)
                {
                    response.message = "AddressType not found";
                    response.statusCode = StatusCodes.Status404NotFound;
                    return Ok(response);
                }

                response.data = data;
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
        [HttpPost("AddAddressType")]
        public async Task<IActionResult> AddAddressType([FromBody] dimAddressTypeTO model)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "AddressType added successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (model == null)
            {
                response.message = "Invalid addresstype data";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = 0;
                return BadRequest(response);
            }

            try
            {
                int result = await _bl.AddAddressTypeBL(model);
                if (result <= 0)
                {
                    response.message = "Failed to add addresstype";
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
        [HttpPut("UpdateAddressType")]
        public async Task<IActionResult> UpdateAddressType([FromBody] dimAddressTypeTO model)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "AddressType updated successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (model == null)
            {
                response.message = "Invalid addresstype data";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = 0;
                return BadRequest(response);
            }

            try
            {
                int result = await _bl.UpdateAddressTypeBL(model);

                if (result <= 0)
                {
                    response.message = "AddressType update failed";
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
        [HttpDelete("DeleteAddressType")]
        public async Task<IActionResult> DeleteAddressType(int idAddressType)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "AddressType deleted successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (idAddressType <= 0)
            {
                response.message = "Invalid addresstype ID";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = 0;
                return BadRequest(response);
            }

            try
            {
                int result = await _bl.DeleteAddressTypeBL(idAddressType);

                if (result <= 0)
                {
                    response.message = "AddressType deletion failed";
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

        #region dimCityTO
        [HttpGet("GetAllCities")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllCities()
        {
            var response = new GlobalResponseModel<List<dimCityTO>>
            {
                message = "Cities retrieved successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            try
            {
                var data = await _bl.GetAllCitiesBL();

                if (data == null || data.Count == 0)
                {
                    response.message = "No cities found";
                    response.statusCode = StatusCodes.Status204NoContent;
                    response.status = "Success";
                    response.data = new List<dimCityTO>();
                    return Ok(response);
                }

                response.data = data;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.exception = ex;
                response.message = Helper.Common.Utility.FormatExceptionMessage(ex);
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status = "Error";
                response.data = new List<dimCityTO>();
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
        [HttpGet("GetCityById")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCityById(int id)
        {
            var response = new GlobalResponseModel<dimCityTO>
            {
                message = "City retrieved successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (id <= 0)
            {
                response.message = "Invalid city ID";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                return BadRequest(response);
            }

            try
            {
                var data = await _bl.GetCityByIdBL(id);

                if (data == null)
                {
                    response.message = "City not found";
                    response.statusCode = StatusCodes.Status404NotFound;
                    return Ok(response);
                }

                response.data = data;
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
        [HttpPost("AddCity")]
        public async Task<IActionResult> AddCity([FromBody] dimCityTO model)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "City added successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (model == null)
            {
                response.message = "Invalid city data";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = 0;
                return BadRequest(response);
            }

            try
            {
                int result = await _bl.AddCityBL(model);
                if (result <= 0)
                {
                    response.message = "Failed to add city";
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
        [HttpPut("UpdateCity")]
        public async Task<IActionResult> UpdateCity([FromBody] dimCityTO model)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "City updated successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (model == null)
            {
                response.message = "Invalid city data";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = 0;
                return BadRequest(response);
            }

            try
            {
                int result = await _bl.UpdateCityBL(model);

                if (result <= 0)
                {
                    response.message = "City update failed";
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
        [HttpDelete("DeleteCity")]
        public async Task<IActionResult> DeleteCity(int idCity)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "City deleted successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (idCity <= 0)
            {
                response.message = "Invalid city ID";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = 0;
                return BadRequest(response);
            }

            try
            {
                int result = await _bl.DeleteCityBL(idCity);

                if (result <= 0)
                {
                    response.message = "City deletion failed";
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

        #region dimCountryTO
        [HttpGet("GetAllCountries")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllCountries()
        {
            var response = new GlobalResponseModel<List<dimCountryTO>>
            {
                message = "Countries retrieved successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            try
            {
                var data = await _bl.GetAllCountriesBL();

                if (data == null || data.Count == 0)
                {
                    response.message = "No countries found";
                    response.statusCode = StatusCodes.Status204NoContent;
                    response.status = "Success";
                    response.data = new List<dimCountryTO>();
                    return Ok(response);
                }

                response.data = data;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.exception = ex;
                response.message = Helper.Common.Utility.FormatExceptionMessage(ex);
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status = "Error";
                response.data = new List<dimCountryTO>();
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
        [HttpGet("GetCountryById")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCountryById(int id)
        {
            var response = new GlobalResponseModel<dimCountryTO>
            {
                message = "Country retrieved successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (id <= 0)
            {
                response.message = "Invalid country ID";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                return BadRequest(response);
            }

            try
            {
                var data = await _bl.GetCountryByIdBL(id);

                if (data == null)
                {
                    response.message = "Country not found";
                    response.statusCode = StatusCodes.Status404NotFound;
                    return Ok(response);
                }

                response.data = data;
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
        [HttpPost("AddCountry")]
        public async Task<IActionResult> AddCountry([FromBody] dimCountryTO model)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "Country added successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (model == null)
            {
                response.message = "Invalid country data";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = 0;
                return BadRequest(response);
            }

            try
            {
                int result = await _bl.AddCountryBL(model);
                if (result <= 0)
                {
                    response.message = "Failed to add country";
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
        [HttpPut("UpdateCountry")]
        public async Task<IActionResult> UpdateCountry([FromBody] dimCountryTO model)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "Country updated successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (model == null)
            {
                response.message = "Invalid country data";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = 0;
                return BadRequest(response);
            }

            try
            {
                int result = await _bl.UpdateCountryBL(model);

                if (result <= 0)
                {
                    response.message = "Country update failed";
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
        [HttpDelete("DeleteCountry")]
        public async Task<IActionResult> DeleteCountry(int idCountry)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "Country deleted successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (idCountry <= 0)
            {
                response.message = "Invalid country ID";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = 0;
                return BadRequest(response);
            }

            try
            {
                int result = await _bl.DeleteCountryBL(idCountry);

                if (result <= 0)
                {
                    response.message = "Country deletion failed";
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

        #region dimStateTO
        [HttpGet("GetAllStates")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllStates()
        {
            var response = new GlobalResponseModel<List<dimStateTO>>
            {
                message = "States retrieved successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            try
            {
                var data = await _bl.GetAllStatesBL();

                if (data == null || data.Count == 0)
                {
                    response.message = "No states found";
                    response.statusCode = StatusCodes.Status204NoContent;
                    response.status = "Success";
                    response.data = new List<dimStateTO>();
                    return Ok(response);
                }

                response.data = data;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.exception = ex;
                response.message = Helper.Common.Utility.FormatExceptionMessage(ex);
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status = "Error";
                response.data = new List<dimStateTO>();
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
        [HttpGet("GetStateById")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStateById(int id)
        {
            var response = new GlobalResponseModel<dimStateTO>
            {
                message = "State retrieved successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (id <= 0)
            {
                response.message = "Invalid state ID";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                return BadRequest(response);
            }

            try
            {
                var data = await _bl.GetStateByIdBL(id);

                if (data == null)
                {
                    response.message = "State not found";
                    response.statusCode = StatusCodes.Status404NotFound;
                    return Ok(response);
                }

                response.data = data;
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
        [HttpPost("AddState")]
        public async Task<IActionResult> AddState([FromBody] dimStateTO model)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "State added successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (model == null)
            {
                response.message = "Invalid state data";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = 0;
                return BadRequest(response);
            }

            try
            {
                int result = await _bl.AddStateBL(model);
                if (result <= 0)
                {
                    response.message = "Failed to add state";
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
        [HttpPut("UpdateState")]
        public async Task<IActionResult> UpdateState([FromBody] dimStateTO model)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "State updated successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (model == null)
            {
                response.message = "Invalid state data";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = 0;
                return BadRequest(response);
            }

            try
            {
                int result = await _bl.UpdateStateBL(model);

                if (result <= 0)
                {
                    response.message = "State update failed";
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
        [HttpDelete("DeleteState")]
        public async Task<IActionResult> DeleteState(int idState)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "State deleted successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (idState <= 0)
            {
                response.message = "Invalid state ID";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = 0;
                return BadRequest(response);
            }

            try
            {
                int result = await _bl.DeleteStateBL(idState);

                if (result <= 0)
                {
                    response.message = "State deletion failed";
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

        #region tblAddressMappingTO
        [HttpGet("GetAllAddressMappings")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllAddressMappings()
        {
            var response = new GlobalResponseModel<List<tblAddressMappingTO>>
            {
                message = "AddressMappings retrieved successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            try
            {
                var data = await _bl.GetAllAddressMappingsBL();

                if (data == null || data.Count == 0)
                {
                    response.message = "No addressmappings found";
                    response.statusCode = StatusCodes.Status204NoContent;
                    response.status = "Success";
                    response.data = new List<tblAddressMappingTO>();
                    return Ok(response);
                }

                response.data = data;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.exception = ex;
                response.message = Helper.Common.Utility.FormatExceptionMessage(ex);
                response.statusCode = StatusCodes.Status500InternalServerError;
                response.status = "Error";
                response.data = new List<tblAddressMappingTO>();
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
        [HttpGet("GetAddressMappingById")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAddressMappingById(int id)
        {
            var response = new GlobalResponseModel<tblAddressMappingTO>
            {
                message = "AddressMapping retrieved successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (id <= 0)
            {
                response.message = "Invalid addressmapping ID";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                return BadRequest(response);
            }

            try
            {
                var data = await _bl.GetAddressMappingByIdBL(id);

                if (data == null)
                {
                    response.message = "AddressMapping not found";
                    response.statusCode = StatusCodes.Status404NotFound;
                    return Ok(response);
                }

                response.data = data;
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
        [HttpPost("AddAddressMapping")]
        public async Task<IActionResult> AddAddressMapping([FromBody] tblAddressMappingTO model)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "AddressMapping added successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (model == null)
            {
                response.message = "Invalid addressmapping data";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = 0;
                return BadRequest(response);
            }

            try
            {
                int result = await _bl.AddAddressMappingBL(model);
                if (result <= 0)
                {
                    response.message = "Failed to add addressmapping";
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
        [HttpPut("UpdateAddressMapping")]
        public async Task<IActionResult> UpdateAddressMapping([FromBody] tblAddressMappingTO model)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "AddressMapping updated successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (model == null)
            {
                response.message = "Invalid addressmapping data";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = 0;
                return BadRequest(response);
            }

            try
            {
                int result = await _bl.UpdateAddressMappingBL(model);

                if (result <= 0)
                {
                    response.message = "AddressMapping update failed";
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
        [HttpDelete("DeleteAddressMapping")]
        public async Task<IActionResult> DeleteAddressMapping(int idAddressMapping)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "AddressMapping deleted successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (idAddressMapping <= 0)
            {
                response.message = "Invalid addressmapping ID";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = 0;
                return BadRequest(response);
            }

            try
            {
                int result = await _bl.DeleteAddressMappingBL(idAddressMapping);

                if (result <= 0)
                {
                    response.message = "AddressMapping deletion failed";
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

    }
}
