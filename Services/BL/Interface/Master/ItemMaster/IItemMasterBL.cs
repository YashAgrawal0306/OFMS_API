using DTO.Models.CommonModel;
using DTO.Models.Master.ItemMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.BL.Interface.Master.ItemMaster
{
    public interface IItemMasterBL
    {
        #region Group Master
        Task<ResultMessage> AddGroupMaster(TblGroupMasterTO groupMaster);
        Task<(List<TblGroupMasterTO>, int)> GetListOfGroupMaster(FilterModelTO filterModelTO);
        Task<ResultMessage> UpdateGroupMaster(TblGroupMasterTO groupMaster);
        Task<ResultMessage> DeleteGroupMaster(int IdGroup);
        Task<TblGroupMasterTO> GetGroupById(int idGroup);
        #endregion

        #region Category Master
        Task<List<TblCategoryMasterTO>> GetListOfCategoryMaster(FilterModelTO filterModelTO);
        Task<ResultMessage> AddCategoryMaster(TblCategoryMasterTO categoryMaster);
        Task<ResultMessage> UpdateCategoryMaster(TblCategoryMasterTO categoryMaster);
        Task<ResultMessage> DeleteCategoryMaster(int IdCategory);
        Task<TblCategoryMasterTO> GetCategoryById(int idCategory);

        #endregion
        Task<List<TblItemMasterTO>> GetListOfItemMaster(FilterModelTO filterModelTO);
    }
}
