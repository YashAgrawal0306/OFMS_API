using DTO.Models.CommonModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.BL.Interface.Master.ItemMasterDropDownBL;

namespace OFMS_API.Controllers.Master.DropDownItemMaster
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemMasterDropDownController : ControllerBase
    {
        private readonly IItemMasterDropDownBL _itemMasterDropDownBL;

        public ItemMasterDropDownController(IItemMasterDropDownBL iDropdownBL)
        {
            _itemMasterDropDownBL = iDropdownBL;
        } 
        #region Group Dropdown

        [HttpPost("GetGroupDropdown")]
        public async Task<IActionResult> GetGroupDropdown(FilterModelTO filterModelTO)
        {
            var response = new GlobalResponseModel<IEnumerable<DropDownList>>
            {
                message = "Group dropdown fetched successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };
            try
            {
                var data = await _itemMasterDropDownBL.GetGroupDropdown(filterModelTO);
                response.data = data;
                response.TotalRecords = data.Count();
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

        #endregion 
        #region Category Dropdown

        [HttpPost("GetCategoryDropdown")]
        public async Task<IActionResult> GetCategoryDropdown(int idGroupMaster, FilterModelTO filterModelTO)
        {
            var response = new GlobalResponseModel<IEnumerable<DropDownList>>
            {
                message = "Category dropdown fetched successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };
            try
            {
                var data = await _itemMasterDropDownBL.GetCategoryDropdown(idGroupMaster, filterModelTO);
                response.data = data;
                response.TotalRecords = data.Count();
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

        #endregion

        // -------------------------------------------------------
        // GET SUB-CATEGORY DROPDOWN (by CategoryId + Filter)
        // -------------------------------------------------------
        #region SubCategory Dropdown

        [HttpPost("GetSubCategoryDropdown")]
        public async Task<IActionResult> GetSubCategoryDropdown(int idCategory, FilterModelTO filterModelTO)
        {
            var response = new GlobalResponseModel<IEnumerable<DropDownList>>
            {
                message = "Sub-category dropdown fetched successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };
            try
            {
                var data = await _itemMasterDropDownBL.GetSubCategoryDropdown(idCategory, filterModelTO);
                response.data = data;
                response.TotalRecords = data.Count();
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

        #endregion

        // -------------------------------------------------------
        // GET ITEM DROPDOWN (by SubCategoryId + Filter)
        // -------------------------------------------------------
        #region Item Dropdown

        [HttpPost("GetItemDropdown")]
        public async Task<IActionResult> GetItemDropdown(int idSubCategory, FilterModelTO filterModelTO)
        {
            var response = new GlobalResponseModel<IEnumerable<DropDownList>>
            {
                message = "Item dropdown fetched successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };
            try
            {
                var data = await _itemMasterDropDownBL.GetItemDropdown(idSubCategory, filterModelTO);
                response.data = data;
                response.TotalRecords = data.Count();
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

        #endregion
    }
}
