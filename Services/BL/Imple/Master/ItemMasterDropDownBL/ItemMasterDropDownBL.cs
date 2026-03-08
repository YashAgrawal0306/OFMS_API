using DTO.Models.CommonModel;
using Repository.DAL.Interface.Master.DropDownItemMaster;
using Services.BL.Interface.Master.ItemMasterDropDownBL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.BL.Imple.Master.ItemMasterDropDownBL
{
    public class ItemMasterDropDownBL : IItemMasterDropDownBL
    {
        private readonly IItemMasterDropDownDAL _itemMasterDropDownDAL;

        public ItemMasterDropDownBL(IItemMasterDropDownDAL itemMasterDropDownDAL)
        {
            _itemMasterDropDownDAL = itemMasterDropDownDAL;
        }

        public async Task<IEnumerable<DropDownList>> GetGroupDropdown(FilterModelTO filterModelTO)
        {
            return await _itemMasterDropDownDAL.GetGroupDropdown(filterModelTO);
        }

        public async Task<IEnumerable<DropDownList>> GetCategoryDropdown(int idGroupMaster, FilterModelTO filterModelTO)
        {
            return await _itemMasterDropDownDAL.GetCategoryDropdown(idGroupMaster, filterModelTO);
        }

        public async Task<IEnumerable<DropDownList>> GetSubCategoryDropdown(int idCategory, FilterModelTO filterModelTO)
        {
            return await _itemMasterDropDownDAL.GetSubCategoryDropdown(idCategory, filterModelTO);
        }

        public async Task<IEnumerable<DropDownList>> GetItemDropdown(int idSubCategory, FilterModelTO filterModelTO)
        {
            return await _itemMasterDropDownDAL.GetItemDropdown(idSubCategory, filterModelTO);
        }
    }
}
