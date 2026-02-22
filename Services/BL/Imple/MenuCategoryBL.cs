using OFMS_API.BL.Interface;
using OFMS_API.DAL.Interface;
using OFMS_API.Models;

namespace OFMS_API.BL.Imple
{

    public class MenuCategoryBL : IMenuCategoryBL
    {
        private readonly IMenuCategoryDAL _menuCategoryDAL;

        public MenuCategoryBL(IMenuCategoryDAL menuCategoryDAL)
        {
            _menuCategoryDAL = menuCategoryDAL;
        }
        public async Task<List<MenuItemsTO>> GatAllMenuItemListBL(FilterModelTO filterModelTO)
        {
            var result = await _menuCategoryDAL.GetAllMenuItemsListDAL(filterModelTO);
            return result;
        }

        public async Task<List<MenuCategoriesTO>> GetCategoriesBL()
        {
            try
            {
                var data = await _menuCategoryDAL.GetAllCategoriesDAL();
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<DropDownList>> GetCategoryDropDownListBL()
        {
            return await _menuCategoryDAL.GetCategoryDropDownListDAL();
        }

        public async Task<int> AddNewCategoryBL(MenuCategoriesTO categories)
        {
            return await _menuCategoryDAL.AddNewCategory(categories);
        }

        public async Task<int> AddNewMenuItem(MenuItemsTO menuItem)
        {
            return await _menuCategoryDAL.AddNewMenuItem(menuItem);
        }

        public async Task<int> EditMenuItemBL(MenuItemsTO menu_Item)
        {
            return await _menuCategoryDAL.EditMenuItemDAL(menu_Item);
        }

        public async Task<int> DeleteMenuItemBL(int menuid)
        {
            return await _menuCategoryDAL.DeleteMenuItemDAL(menuid);
        }

        public async Task<int> AddDublicateMenuItemBL(CopyDublicateItemTO itemTO)
        {
            return await _menuCategoryDAL.AddDublicateMenuItemDAL(itemTO);
        }
    }
}