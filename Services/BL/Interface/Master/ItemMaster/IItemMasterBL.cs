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
        #region Get Methods
        Task<(List<TblGroupMasterTO>, int)> GetListOfGroupMaster(FilterModelTO filterModelTO);
        Task<TblGroupMasterTO> GetGroupById(int idGroup);
        Task<List<TblCategoryMasterTO>> GetListOfCategoryMaster(FilterModelTO filterModelTO);
        Task<List<TblItemMasterTO>> GetListOfItemMaster(FilterModelTO filterModelTO);


        #endregion
        #region Add Methods
        Task<ResultMessage> AddGroupMaster(TblGroupMasterTO groupMaster);
        #endregion
        Task<ResultMessage> UpdateGroupMaster(TblGroupMasterTO groupMaster);
        Task<ResultMessage> DeleteGroupMaster(int IdGroup);

    }
}
