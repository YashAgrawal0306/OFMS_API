using OFMS_API.Models;

namespace OFMS_API.BL.Interface
{
    public interface IMenuCategoryBL
    {
        Task<List<menu_item>> GatAllMenuItemListBL();
        List<menu_categories> GetCategoriesBL();

        int AddNewCategoryBL(menu_categories categories);
        int AddNewMenuItem(menu_item menu_Item);
        int AddDublicateMenuItemBL(CopyDublicateItemTO itemTO);

        Task<int> EditMenuItemBL(menu_item menu_Item);


        int DeleteMenuItemBL(int menuid);
    }
}
