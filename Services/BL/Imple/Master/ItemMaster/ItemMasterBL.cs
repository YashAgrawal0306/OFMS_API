using DTO.Models.CommonModel;
using DTO.Models.Master.ItemMaster;
using DTO.Models.Master.ItemMaster.ResponseModel;
using Repository.DAL.Interface.Master.ImageMaster;
using Repository.DAL.Interface.Master.ItemMaster;
using Services.BL.Interface.Master.ImageMaster;
using Services.BL.Interface.Master.ItemMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Helper.Helper.Common.Enums;

namespace Services.BL.Imple.Master.ItemMaster
{
    public class ItemMasterBL : IItemMasterBL
    {
        private readonly IItemMasterDAL _itemMasterDAL;
        private readonly IImageMasterDAL _imageMasterDAL;
        public ItemMasterBL(IItemMasterDAL itemMasterDAL,IImageMasterDAL imageMasterDAL)
        {
            _itemMasterDAL = itemMasterDAL;
            _imageMasterDAL = imageMasterDAL;
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

        public async Task<(List<TblGroupMasterResponseTO>, int)> GetListOfGroupMaster(FilterModelTO filterModelTO)
        {
            var data = await _itemMasterDAL.GetListOfGroupMaster(filterModelTO);
            int ImageTypeId = (int)ImageType.GROUP;
            if (data.Item1 != null && data.Item2 >0)
            {
                foreach(var item in data.Item1)
                {
                   item.AttachmentTo =await _imageMasterDAL.GetItemMasterImageByReferenceId(item.IdGroupMaster, ImageTypeId);
                }
            }
            return data; 
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

        public async Task<TblGroupMasterResponseTO> GetGroupById(int idGroup)
        {
            TblGroupMasterResponseTO tblGroupMasterResponseTO = new();
            var data =await _itemMasterDAL.GetGroupById(idGroup);
            int ImageTypeId = (int)ImageType.GROUP;
            if (data != null)
            {
                tblGroupMasterResponseTO.UpdatedOn = data.UpdatedOn;
                tblGroupMasterResponseTO.UpdatedBy = data.UpdatedBy;
                tblGroupMasterResponseTO.IdGroupMaster = data.IdGroupMaster;
                tblGroupMasterResponseTO.GroupName = data.GroupName; 
                tblGroupMasterResponseTO.CreatedBy = data.CreatedBy;
                tblGroupMasterResponseTO.CreatedOn = data.CreatedOn;
                tblGroupMasterResponseTO.Description = data.Description;
                tblGroupMasterResponseTO.IsActive = data.IsActive;
                tblGroupMasterResponseTO.CreatedByName = data.CreatedByName;
                tblGroupMasterResponseTO.UpdatedByName = data.UpdatedByName;
                tblGroupMasterResponseTO.AttachmentTo = await _imageMasterDAL.GetItemMasterImageByReferenceId(data.IdGroupMaster, ImageTypeId); 
            }
            return tblGroupMasterResponseTO;
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
        public async Task<OutPutClass<CategoryWithSubCategoryListTO>> GetCategoryWithSubCategoryList(FilterModelTO filterModelTO)
        {
            var output = new OutPutClass<CategoryWithSubCategoryListTO>();
             
            var parentFilter = new FilterModelTO
            {
                PageNo = filterModelTO.PageNo,
                PageSize = filterModelTO.PageSize,
                SearchText = filterModelTO.SearchText,
                isActive = filterModelTO.isActive,
                CategoryId = filterModelTO.CategoryId,
                Flag = "1"
            };
             
            var subFilter = new FilterModelTO
            {
                PageNo = 0,    
                PageSize = 0,
                isActive = filterModelTO.isActive,
                Flag = "2"
            };

            var parentResult = await _itemMasterDAL.GetListOfCategoryMaster(parentFilter);
            var subResult = await _itemMasterDAL.GetListOfCategoryMaster(subFilter);
             
            var subGrouped = subResult?.List?
                .GroupBy(s => s.ParentId ?? 0)
                .ToDictionary(g => g.Key, g => g.ToList());
             
            output.List = parentResult?.List?.Select(parent => new CategoryWithSubCategoryListTO
            {
                IdCategory = parent.IdCategory,
                IdGroupMaster = parent.IdGroupMaster,
                ParentId = parent.ParentId,
                CategoryName = parent.CategoryName,
                CatDescription = parent.CatDescription,
                IsActive = parent.IsActive,
                CreatedAt = parent.CreatedAt,
                CreatedBy = parent.CreatedBy,
                UpdatedAt = parent.UpdatedAt,
                UpdatedBy = parent.UpdatedBy,
                GroupName =parent.GroupName,
                SubCategoryList = subGrouped != null && subGrouped.TryGetValue(parent.IdCategory, out var subs)
                ? subs.Select(s => new SubCategoryList
                {
                    IdSubCategory = s.IdCategory,
                    SubCategoryName = s.CategoryName ?? string.Empty,
                    SubCategoryDescription = s.CatDescription ?? string.Empty,
                    IsActive = s.IsActive
                }).ToList()
                : new List<SubCategoryList>(),
                totalSubCount = subGrouped != null && subGrouped.TryGetValue(parent.IdCategory, out var subCount) ? subCount.Count  : 0,

            }).ToList();

            output.TotalCount = parentResult?.TotalCount; // total parent count for pagination
            
            return output;
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
                resultMessage.Errors.Add("No record found t o update or update operation failed");
            }

            return resultMessage;
        } 
        public async Task<TblCategoryMasterTO> GetCategoryById(int idCategory)
        {
            return await _itemMasterDAL.GetCategoryById(idCategory);
        }
        public async Task<ViewTblCategoryMasterTO> GetCategoryWithSubCatById(int idCategory)
        {
            // 1️⃣ Fetch the category
            var data = await _itemMasterDAL.GetCategoryById(idCategory);

            // 2️⃣ Map to ViewTblCategoryMasterTO
            var result = new ViewTblCategoryMasterTO
            {
                IdCategory = data.IdCategory,
                IdGroupMaster = data.IdGroupMaster,
                ParentId = data.ParentId,
                CategoryName = data.CategoryName,
                CatDescription = data.CatDescription,
                IsActive = data.IsActive,
                CreatedAt = data.CreatedAt,
                CreatedBy = data.CreatedBy,
                CreatedByName = data.CreatedByName,
                UpdatedAt = data.UpdatedAt,
                UpdatedBy = data.UpdatedBy,
                UpdatedByName= data.UpdatedByName
            };

            // 3️⃣ Fetch sub-categories only if it's a parent (ParentId is null or 0)
            bool isParent = data.ParentId == null || data.ParentId == 0;
            if (isParent)
            {
                var subFilter = new FilterModelTO
                {
                    PageNo = 0,
                    PageSize = 0,
                    isActive = data.IsActive,
                    CategoryId = data.IdCategory,
                    Flag = "2"
                };

                var subResult = await _itemMasterDAL.GetListOfCategoryMaster(subFilter);

                var subList = subResult?.List ?? new List<TblCategoryMasterTO>();

                result.SubCategoryList = subList.Select(s => new SubCategoryList
                {
                    IdSubCategory = s.IdCategory,
                    SubCategoryName = s.CategoryName ?? string.Empty,
                    SubCategoryDescription = s.CatDescription ?? string.Empty,
                    IsActive = s.IsActive
                }).ToList();

                result.TotalSubCategoryCount = subList.Count;
                result.TotalActiveSubCategory = subList.Count(s => s.IsActive);
                result.TotalInActiveSubCategory = subList.Count(s => !s.IsActive);
            }

            return result;
        }

        public async Task<ViewTblSubCategoryWithItemsTO> GetSubCategoryWithItemsById(int idSubCategory)
        {
            // 1️⃣ Fetch the subcategory (reuse same GetCategoryById from DAL)
            var data = await _itemMasterDAL.GetCategoryById(idSubCategory);

            // 2️⃣ Map to ViewTblSubCategoryWithItemsTO
            var result = new ViewTblSubCategoryWithItemsTO
            {
                IdSubCategory = data.IdCategory,
                IdGroupMaster = data.IdGroupMaster,
                ParentId = data.ParentId,
                SubCategoryName = data.CategoryName,
                SubCategoryDescription = data.CatDescription,
                IsActive = data.IsActive,
                CreatedAt = data.CreatedAt,
                CreatedBy = data.CreatedBy,
                CreatedByName = data.CreatedByName,
                UpdatedAt = data.UpdatedAt,
                UpdatedBy = data.UpdatedBy,
                UpdatedByName = data.UpdatedByName
            };

            // 3️⃣ Fetch items for this subcategory
            var itemFilter = new FilterModelTO
            {
                PageNo = 0,
                PageSize = 0,
                isActive = data.IsActive,
                CategoryId = data.IdCategory,
                Flag = "0"
            };

            var itemResult = await _itemMasterDAL.GetItemsBySubCategoryId(itemFilter);
            var itemList = itemResult?.List ?? new List<TblItemMasterTO>();

            // 4️⃣ Map to ItemList and set summary counts
            result.ItemList = itemList.Select(i => new ItemList
            {
                IdItemMaster = i.IdItemMaster,
                ItemName = i.ItemName ?? string.Empty,
                ItemDescription = i.ItemDescription ?? string.Empty,
                Price = i.Price,
                Quantity = i.Quantity,
                IsActive = i.IsActive
            }).ToList();

            result.TotalItemCount = itemList.Count;
            result.TotalActiveItem = itemList.Count(i => i.IsActive);
            result.TotalInActiveItem = itemList.Count(i => !i.IsActive);

            return result;
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
