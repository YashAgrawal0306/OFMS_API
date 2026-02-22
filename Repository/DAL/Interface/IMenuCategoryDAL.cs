using OFMS_API.Models;

namespace OFMS_API.DAL.Interface
{
    public interface IMenuCategoryDAL
    {
        Task<List<MenuCategoriesTO>> GetAllCategoriesDAL();
        Task<List<MenuItemsTO>> GetAllMenuItemsListDAL(FilterModelTO filterModelTO);
        Task<List<DropDownList>> GetCategoryDropDownListDAL();
        Task<int> AddNewCategory(MenuCategoriesTO categories);
        Task<int> AddNewMenuItem(MenuItemsTO menu_Item);
        Task<int> AddDublicateMenuItemDAL(CopyDublicateItemTO itemTO);

        Task<int> EditMenuItemDAL(MenuItemsTO item);

        Task<int> DeleteMenuItemDAL(int menuid);
    }
}
