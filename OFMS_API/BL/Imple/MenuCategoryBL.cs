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
        public Task<List<menu_item>> GatAllMenuItemListBL()
        {
            var result = _menuCategoryDAL.GetAllMenuItemsListDAL();
            return result;
        }

        public async Task<List<menu_categories>> GetCategoriesBL()
        {
            return await _menuCategoryDAL.GetAllCategoriesDAL();
        }


        public async Task<int> AddNewCategoryBL(menu_categories categories)
        {
           return await _menuCategoryDAL.AddNewCategory(categories);
        }

        public async Task<int> AddNewMenuItem(menu_item menuItem)
        {
            return await _menuCategoryDAL.AddNewMenuItem(menuItem);
        }

        public async Task<int> EditMenuItemBL(menu_item menu_Item)
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
