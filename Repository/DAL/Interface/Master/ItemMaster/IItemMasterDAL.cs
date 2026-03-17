using DTO.Models.CommonModel;
using DTO.Models.Master.ItemMaster;
using DTO.Models.Master.ItemMaster.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DAL.Interface.Master.ItemMaster
{
    public interface IItemMasterDAL
    {
        #region Group Master
        Task<int> AddGroupMaster(TblGroupMasterTO groupMaster);
        Task<(List<TblGroupMasterResponseTO>, int)> GetListOfGroupMaster(FilterModelTO filterModelTO);
        Task<TblGroupMasterTO> GetGroupById(int IdGroup);
        Task<int> UpdateGroupMaster(TblGroupMasterTO groupMaster);
        Task<int> DeleteGroupMaster(int idGroup);

        #endregion

        #region Category Master
        Task<OutPutClass<TblCategoryMasterTO>> GetListOfCategoryMaster(FilterModelTO filterModelTO);
        Task<OutPutClass<TblItemMasterTO>> GetItemsBySubCategoryId(FilterModelTO filterModelTO);
        Task<int> AddCategoryMaster(TblCategoryMasterTO categoryMaster);
        Task<int> UpdateCategoryMaster(TblCategoryMasterTO categoryMaster);
        Task<TblCategoryMasterTO> GetCategoryById(int IdCategory);
        #endregion

        #region Item Master
        Task<int> AddItemMaster(TblItemMasterTO model);
        Task<OutPutClass<TblItemMasterTO>> GetListOfItemMaster(FilterModelTO filterModelTO);
        Task<TblItemMasterTO> GetItemMasterById(int id);
        Task<int> UpdateItemMaster(TblItemMasterTO model);

        #endregion
    }
}
