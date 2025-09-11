using OFMS_API.Models;

namespace OFMS_API.DAL.Interface
{
    public interface IMenuCategoryDAL
    {
        Task<List<menu_categories>> GetAllCategoriesDAL();
        Task<List<menu_item>> GetAllMenuItemsListDAL();

        Task<int> AddNewCategory(menu_categories categories);
        Task<int> AddNewMenuItem(menu_item menu_Item);
        //Task<int> AddDublicateMenuItemDAL(CopyDublicateItemTO itemTO);

        Task<int> EditMenuItemDAL(menu_item item);

        Task<int> DeleteMenuItemDAL(int menuid);
    }
}
