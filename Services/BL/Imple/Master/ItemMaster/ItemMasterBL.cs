using DTO.Models.CommonModel;
using DTO.Models.Master.ItemMaster;
using Repository.DAL.Interface.Master.ItemMaster;
using Services.BL.Interface.Master.ItemMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.BL.Imple.Master.ItemMaster
{
    public class ItemMasterBL : IItemMasterBL
    {
        private readonly IItemMasterDAL _itemMasterDAL;
        public ItemMasterBL(IItemMasterDAL itemMasterDAL)
        {
            _itemMasterDAL = itemMasterDAL;
        }
        public async Task<ResultMessage> AddGroupMaster(TblGroupMasterTO groupMaster)
        {
            ResultMessage resultMessage = new();
            int id =await _itemMasterDAL.AddGroupMaster(groupMaster);
            if (id == 0) { 
                resultMessage.Errors.Add("Failed to add group master");
                resultMessage.IsSuccess = false;
                resultMessage.Message = "Failed to add group master";
                resultMessage.Data = 0;
            }
            resultMessage.Data = id;
            return resultMessage;
        }

        public Task<List<TblCategoryMasterTO>> GetListOfCategoryMaster(FilterModelTO filterModelTO)
        {
            throw new NotImplementedException();
        }

        public async Task<(List<TblGroupMasterTO>, int)> GetListOfGroupMaster(FilterModelTO filterModelTO)
        {
            return await _itemMasterDAL.GetListOfGroupMaster(filterModelTO);
        }

        public Task<List<TblItemMasterTO>> GetListOfItemMaster(FilterModelTO filterModelTO)
        {
            throw new NotImplementedException();
        }
    }
}
