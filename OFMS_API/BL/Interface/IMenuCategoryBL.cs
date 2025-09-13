using OFMS_API.Models;

namespace OFMS_API.BL.Interface
{
    public interface IMenuCategoryBL
    {
        Task<List<MenuItemsTO>> GatAllMenuItemListBL();
        Task<List<MenuCategoriesTO>> GetCategoriesBL();

        Task<int> AddNewCategoryBL(MenuCategoriesTO categories);
        Task<int> AddNewMenuItem(MenuItemsTO menu_Item);
        Task<int> AddDublicateMenuItemBL(CopyDublicateItemTO itemTO);

        Task<int> EditMenuItemBL(MenuItemsTO menu_Item);


        Task<int> DeleteMenuItemBL(int menuid);
    }
}
