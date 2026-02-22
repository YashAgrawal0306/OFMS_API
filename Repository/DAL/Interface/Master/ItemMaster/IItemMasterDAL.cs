using DTO.Models.CommonModel;
using DTO.Models.Master.ItemMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DAL.Interface.Master.ItemMaster
{
    public interface IItemMasterDAL
    {
        #region Get Methods
        Task<(List<TblGroupMasterTO>, int)> GetListOfGroupMaster(FilterModelTO filterModelTO);
        Task<TblGroupMasterTO> GetGroupById(int IdGroup);
        Task<List<TblCategoryMasterTO>> GetListOfCategoryMaster(FilterModelTO filterModelTO);
        Task<List<TblItemMasterTO>> GetListOfItemMaster(FilterModelTO filterModelTO);
        #endregion
        #region Add Methods
        Task<int> AddGroupMaster(TblGroupMasterTO groupMaster);
        #endregion
        Task<int> UpdateGroupMaster(TblGroupMasterTO groupMaster);
        Task<int> DeleteGroupMaster(int idGroup);
    }
}
