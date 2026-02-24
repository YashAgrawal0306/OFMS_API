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

        #region Group Master
        public async Task<ResultMessage> AddGroupMaster(TblGroupMasterTO groupMaster)
        {
            ResultMessage resultMessage = new();
            int id = await _itemMasterDAL.AddGroupMaster(groupMaster);
            if (id == 0)
            {
                resultMessage.Errors.Add("Failed to add group master");
                resultMessage.IsSuccess = false;
                resultMessage.Message = "Failed to add group master";
                resultMessage.Data = 0;
            }
            resultMessage.Data = id;
            return resultMessage;
        }

        public async Task<(List<TblGroupMasterTO>, int)> GetListOfGroupMaster(FilterModelTO filterModelTO)
        {
            return await _itemMasterDAL.GetListOfGroupMaster(filterModelTO);
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

            return new ResultMessage
            {
                IsSuccess = true,
                Message = "Group master updated successfully.",
                StatusCode = 200,
                Data = rowsAffected
            };
        }
        #endregion

        #region CategoryMaster
        public async Task<OutPutClass<TblCategoryMasterTO>> GetListOfCategoryMaster(FilterModelTO filterModelTO)
        {
            return await _itemMasterDAL.GetListOfCategoryMaster(filterModelTO);
        }

        public async Task<ResultMessage> AddCategoryMaster(TblCategoryMasterTO categoryMaster)
        {
            ResultMessage resultMessage = new();

            if (categoryMaster.ParentId == null || categoryMaster.ParentId == 0)
            {
                categoryMaster.ParentId = 0; // Category
            }
            else
            {
                if (categoryMaster.ParentId <= 0)
                {
                    resultMessage.IsSuccess = false;
                    resultMessage.Message = "Invalid parent category";
                    return resultMessage;
                }
            }

            categoryMaster.CreatedAt = DateTime.Now;
            categoryMaster.IsActive = true;

            int id = await _itemMasterDAL.AddCategoryMaster(categoryMaster);

            if (id > 0)
            {
                resultMessage.IsSuccess = true;
                resultMessage.Message = categoryMaster.ParentId == 0
                    ? "Category added successfully"
                    : "Sub-category added successfully";
                resultMessage.Data = id;
            }
            else
            {
                resultMessage.IsSuccess = false;
                resultMessage.Message = categoryMaster.ParentId == 0
                    ? "Failed to add category"
                    : "Failed to add sub-category";
            }

            return resultMessage;
        }

        public async Task<ResultMessage> UpdateCategoryMaster(TblCategoryMasterTO categoryMaster)
        {
            ResultMessage resultMessage = new();
            int id = await _itemMasterDAL.UpdateCategoryMaster(categoryMaster);

            if (id > 0)
            {
                resultMessage.IsSuccess = true;
                resultMessage.Message = "Category updated successfully";
                resultMessage.StatusCode = 200;
            }
            else
            {
                resultMessage.IsSuccess = false;
                resultMessage.Message = "Failed to update category";
                resultMessage.StatusCode = 500;
                resultMessage.Errors.Add("No record found to update or update operation failed");
            }

            return resultMessage;
        } 
        public async Task<TblCategoryMasterTO> GetCategoryById(int idCategory)
        {
            return await _itemMasterDAL.GetCategoryById(idCategory);
        }

        #endregion

        #region Item Master 
        public async Task<ResultMessage> AddItemMaster(TblItemMasterTO model)
        {
            ResultMessage resultMessage = new();
            int id = await _itemMasterDAL.AddItemMaster(model);

            if (id > 0)
            {
                resultMessage.IsSuccess = true;
                resultMessage.Message = "Item added successfully";
                resultMessage.StatusCode = 200;
            }
            else
            {
                resultMessage.IsSuccess = false;
                resultMessage.Message = "Failed to add item";
                resultMessage.StatusCode = 500;
                resultMessage.Errors.Add("Insert operation failed");
            }

            return resultMessage;
        }
         
        public async Task<OutPutClass<TblItemMasterTO>> GetListOfItemMaster(FilterModelTO filterModelTO)
        {
            try
            {
                return await _itemMasterDAL.GetListOfItemMaster(filterModelTO);
            }
            catch (Exception)
            {
                throw;
            }
        } 
        public async Task<TblItemMasterTO> GetItemMasterById(int id)
        {
            try
            {
                return await _itemMasterDAL.GetItemMasterById(id);
            }
            catch (Exception)
            {
                throw;
            }
        } 
        public async Task<ResultMessage> UpdateItemMaster(TblItemMasterTO model)
        {
            ResultMessage resultMessage = new();
            int id = await _itemMasterDAL.UpdateItemMaster(model);

            if (id > 0)
            {
                resultMessage.IsSuccess = true;
                resultMessage.Message = "Item updated successfully";
                resultMessage.StatusCode = 200;
            }
            else
            {
                resultMessage.IsSuccess = false;
                resultMessage.Message = "Failed to update item";
                resultMessage.StatusCode = 500;
                resultMessage.Errors.Add("No record found to update or update operation failed");
            }

            return resultMessage;
        }
        #endregion

    }
}
