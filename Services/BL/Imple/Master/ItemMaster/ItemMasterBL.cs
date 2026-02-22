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

        public async Task<ResultMessage> DeleteGroupMaster(int IdGroup)
        {
            if (IdGroup <= 0)
                return new ResultMessage
                {
                    IsSuccess = false,
                    Message = "Invalid group master ID.",
                    StatusCode = 400,
                    Errors = ["IdGroupMaster must be greater than 0."]
                };

            var rowsAffected = await _itemMasterDAL.DeleteGroupMaster(IdGroup);

            if (rowsAffected == 0)
                return new ResultMessage
                {
                    IsSuccess = false,
                    Message = "No record found to delete.",
                    StatusCode = 404,
                    Errors = ["No matching record found for the given IdGroupMaster."]
                };

            return new ResultMessage
            {
                IsSuccess = true,
                Message = "Group master deleted successfully.",
                StatusCode = 200
            };
        }

        public Task<TblGroupMasterTO> GetGroupById(int idGroup)
        {
            return _itemMasterDAL.GetGroupById(idGroup);
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

        public async Task<ResultMessage> UpdateGroupMaster(TblGroupMasterTO groupMaster)
        {
            if (groupMaster == null)
                return new ResultMessage
                {
                    IsSuccess = false,
                    Message = "Request body cannot be null.",
                    StatusCode = 400,
                    Errors = ["Group master object is null."]
                };

            if (groupMaster.IdGroupMaster <= 0)
                return new ResultMessage
                {
                    IsSuccess = false,
                    Message = "Invalid group master ID.",
                    StatusCode = 400,
                    Errors = ["IdGroupMaster must be greater than 0."]
                };

            if (string.IsNullOrWhiteSpace(groupMaster.GroupName))
                return new ResultMessage
                {
                    IsSuccess = false,
                    Message = "Group name is required.",
                    StatusCode = 400,
                    Errors = ["GroupName cannot be empty."]
                };

            var rowsAffected = await _itemMasterDAL.UpdateGroupMaster(groupMaster);

            if (rowsAffected == 0)
                return new ResultMessage
                {
                    IsSuccess = false,
                    Message = "No record found to update.",
                    StatusCode = 404,
                    Errors = ["No matching record found for the given IdGroupMaster."]
                };

            return new ResultMessage{
                IsSuccess = true,
                Message = "Group master updated successfully.",
                StatusCode = 200,
                Data = rowsAffected
            };
        }
    }
}
