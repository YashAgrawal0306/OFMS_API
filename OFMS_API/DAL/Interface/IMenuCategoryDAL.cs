using OFMS_API.Models;

namespace OFMS_API.DAL.Interface
{
    public interface IMenuCategoryDAL
    {
        List<menu_categories> GetAllCategoriesDAL();
        Task<List<menu_item>> GetAllMenuItemsListDAL();

        int AddNewCategory(menu_categories categories);
        int AddNewMenuItem(menu_item menu_Item);
        int AddDublicateMenuItemDAL(CopyDublicateItemTO itemTO);

        Task<int> EditMenuItemDAL(menu_item item);

        int DeleteMenuItemDAL(int menuid);
    }
}
