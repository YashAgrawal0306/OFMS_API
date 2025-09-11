using OFMS_API.Models;

namespace OFMS_API.BL.Interface
{
    public interface IMenuCategoryBL
    {
        Task<List<menu_item>> GatAllMenuItemListBL();
        Task<List<menu_categories>> GetCategoriesBL();

        Task<int> AddNewCategoryBL(menu_categories categories);
        Task<int> AddNewMenuItem(menu_item menu_Item);
        //Task<int> AddDublicateMenuItemBL(CopyDublicateItemTO itemTO);

        Task<int> EditMenuItemBL(menu_item menu_Item);


        Task<int> DeleteMenuItemBL(int menuid);
    }
}
