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

        public List<menu_categories> GetCategoriesBL()
        {
            return _menuCategoryDAL.GetAllCategoriesDAL();
        }


        public int AddNewCategoryBL(menu_categories categories)
        {
           return _menuCategoryDAL.AddNewCategory(categories);
        }

        public int AddNewMenuItem(menu_item menuItem)
        {
            return _menuCategoryDAL.AddNewMenuItem(menuItem);
        }

        public Task<int> EditMenuItemBL(menu_item menu_Item)
        {
            return _menuCategoryDAL.EditMenuItemDAL(menu_Item);
        }

        public int DeleteMenuItemBL(int menuid)
        {
            return _menuCategoryDAL.DeleteMenuItemDAL(menuid);
        }

        public int AddDublicateMenuItemBL(CopyDublicateItemTO itemTO)
        {
            return _menuCategoryDAL.AddDublicateMenuItemBL(itemTO);
        }
    }
}
