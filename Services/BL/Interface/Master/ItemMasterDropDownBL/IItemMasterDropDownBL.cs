using DTO.Models.CommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.BL.Interface.Master.ItemMasterDropDownBL
{
    public interface IItemMasterDropDownBL
    {
        Task<IEnumerable<DropDownList>> GetGroupDropdown(FilterModelTO filterModelTO);
        Task<IEnumerable<DropDownList>> GetPaymentType(FilterModelTO filterModelTO);
        Task<IEnumerable<DropDownList>> GetCategoryDropdown(int idGroupMaster, FilterModelTO filterModelTO);
        Task<IEnumerable<DropDownList>> GetSubCategoryDropdown(int idCategory, FilterModelTO filterModelTO);
        Task<IEnumerable<DropDownList>> GetItemDropdown(int idSubCategory, FilterModelTO filterModelTO);
        Task<IEnumerable<DropDownList>> GetStatusByIdTransation(int IdTransactionType, FilterModelTO filterModelTO);
        Task<IEnumerable<DropDownList>> GetUserDropDownList(int idRole, FilterModelTO filterModelTO);
    }
}
