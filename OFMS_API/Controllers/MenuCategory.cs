using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OFMS_API.BL.Interface;
using OFMS_API.Models;

namespace OFMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuCategory : ControllerBase
    {
        #region ctor
        private readonly IMenuCategoryBL _bl;
        public MenuCategory(IMenuCategoryBL menuCategory)
        {
            _bl = menuCategory;
        }
        #endregion

        #region Get
        #region Get All Category
        [HttpGet("GetAllCategory")]
        public async Task<IActionResult> GetAllCategory()
        {
            var response = new GlobalResponseModel<List<menu_categories>>
            {
                message = "Categories retrieved successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            try
            {
                var categories = await _bl.GetCategoriesBL();

                if (categories == null || categories.Count == 0)
                {
                    response.message = "No categories found";
                    response.statusCode = StatusCodes.Status204NoContent;
                    response.status = "Success";
                    response.data = [];
                    return Ok(response);
                }

                response.data = categories;
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

        #region Get All MenuItem
        [HttpGet("GetAllMenuItemList")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllMenuItemList()
        {
            var response = new GlobalResponseModel<List<menu_item>>
            {
                message = "Menu items retrieved successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            try
            {
                var menuList = await _bl.GatAllMenuItemListBL().ConfigureAwait(false);

                if (menuList == null || !menuList.Any())
                {
                    response.message = "No menu items found";
                    response.statusCode = StatusCodes.Status204NoContent;
                    response.status = "Success";
                    response.data = [];
                    return Ok(response);
                }

                response.data = menuList.ToList();
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

        #endregion

        #region Post

        #region AddNewMenuItem 
        [HttpPost("AddNewMenuItem")]
        public async Task<IActionResult> AddNewMenuItem([FromBody] menu_item menu_Item)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "Menu item added successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (menu_Item == null || string.IsNullOrWhiteSpace(menu_Item.MenuName))
            {
                response.message = "Invalid menu item data";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = 0;
                return BadRequest(response);
            }

            try
            {
                int result = await Task.Run(() => _bl.AddNewMenuItem(menu_Item)).ConfigureAwait(false);

                if (result == 0)
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

        #region AddNewCategory

        [HttpPost("AddNewCategory")]
        public async Task<IActionResult> AddNewCategory([FromBody] menu_categories menu)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "Category added successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (menu == null || string.IsNullOrWhiteSpace(menu.name))
            {
                response.message = "Invalid category data";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = 0;
                return BadRequest(response);
            }
            try
            {
                int result =await _bl.AddNewCategoryBL(menu);
                if (result <= 0)
                {
                    response.message = "Failed to add category";
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

        #region
        [HttpPost("AddDublicateMenuItem")]
        public async Task<IActionResult> AddDublicateMenuItem(CopyDublicateItemTO itemTO)
        {
            return Ok();
        }
        #endregion

        #endregion

        #region Edit

        #region EditMenuItems
        [HttpPost("EditMenuItems")]
        public async Task<IActionResult> EditMenuItems([FromBody] menu_item item)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "Menu item updated successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            if (item == null || item.MenuItemId <= 0 || string.IsNullOrWhiteSpace(item.MenuName))
            {
                response.message = "Invalid menu item data";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = 0;
                return BadRequest(response);
            }
            try
            {
                int result = await _bl.EditMenuItemBL(item).ConfigureAwait(false);

                if (result <= 0)
                {
                    response.message = "Menu item update failed";
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

        #endregion

        #region Delete
        #region Menu Management

        [HttpDelete("DeleteMenuItem")]
        public async Task<IActionResult> DeleteMenuItem(int menuid)
        {
            var response = new GlobalResponseModel<int>
            {
                message = "Menu item deleted successfully",
                statusCode = StatusCodes.Status200OK,
                status = "Success"
            };

            // Early return for invalid ID
            if (menuid <= 0)
            {
                response.message = "Invalid menu ID";
                response.statusCode = StatusCodes.Status400BadRequest;
                response.status = "Fail";
                response.data = 0;
                return BadRequest(response);
            }

            try
            {
                int result = await _bl.DeleteMenuItemBL(menuid);

                if (result <= 0)
                {
                    response.message = "Menu item deletion failed";
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

        #endregion
    }
}
