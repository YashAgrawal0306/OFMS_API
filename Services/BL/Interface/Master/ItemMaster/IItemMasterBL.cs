using DTO.Models.CommonModel;
using DTO.Models.Master.ItemMaster;
using DTO.Models.Master.ItemMaster.ResponseModel;
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
        Task<(List<TblGroupMasterResponseTO>, int)> GetListOfGroupMaster(FilterModelTO filterModelTO);
        Task<ResultMessage> UpdateGroupMaster(TblGroupMasterTO groupMaster);
        Task<ResultMessage> DeleteGroupMaster(int IdGroup);
        Task<TblGroupMasterResponseTO> GetGroupById(int idGroup);
        #endregion

        #region Category Master
        Task<OutPutClass<TblCategoryMasterTO>> GetListOfCategoryMaster(FilterModelTO filterModelTO);
        Task<OutPutClass<CategoryWithSubCategoryListTO>> GetCategoryWithSubCategoryList(FilterModelTO filterModelTO);
        Task<ResultMessage> AddCategoryMaster(TblCategoryMasterTO categoryMaster);
        Task<ResultMessage> UpdateCategoryMaster(TblCategoryMasterTO categoryMaster); 
        Task<TblCategoryMasterTO> GetCategoryById(int idCategory);
        Task<ViewTblCategoryMasterTO> GetCategoryWithSubCatById(int idCategory);
        Task<ViewTblSubCategoryWithItemsTO> GetSubCategoryWithItemsById(int idSubCategory);

        #endregion

        #region Item Master
        Task<ResultMessage> AddItemMaster(TblItemMasterTO model);
        Task<OutPutClass<TblItemMasterTO>> GetListOfItemMaster(FilterModelTO filterModelTO);
        Task<TblItemMasterTO> GetItemMasterById(int id);
        Task<ResultMessage> UpdateItemMaster(TblItemMasterTO model);
        #endregion
    }
}
