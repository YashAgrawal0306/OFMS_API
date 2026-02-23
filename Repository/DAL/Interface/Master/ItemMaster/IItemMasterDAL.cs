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
        #region Group Master
        Task<int> AddGroupMaster(TblGroupMasterTO groupMaster);
        Task<(List<TblGroupMasterTO>, int)> GetListOfGroupMaster(FilterModelTO filterModelTO);
        Task<TblGroupMasterTO> GetGroupById(int IdGroup);
        Task<int> UpdateGroupMaster(TblGroupMasterTO groupMaster);
        Task<int> DeleteGroupMaster(int idGroup);
        #endregion

        #region Category Master
        Task<List<TblCategoryMasterTO>> GetListOfCategoryMaster(FilterModelTO filterModelTO);
        Task<int> AddCategoryMaster(TblCategoryMasterTO categoryMaster);
        Task<int> UpdateCategoryMaster(TblCategoryMasterTO categoryMaster);
        Task<int> DeleteCategoryMaster(int idCategory);
        Task<TblCategoryMasterTO> GetCategoryById(int IdCategory);
        #endregion

        Task<List<TblItemMasterTO>> GetListOfItemMaster(FilterModelTO filterModelTO);


  
    }
}
