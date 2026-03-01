using DTO.Models.CommonModel;
using DTO.Models.Master.ImageMaster;
using Microsoft.AspNetCore.Http;
using Repository.DAL.Interface.Master.ImageMaster;
using Services.BL.Interface.Master.ImageMaster; 
using static Helper.Helper.Common.Enums;

namespace Services.BL.Imple.Master.ImageMaster
{
    public class ImageMasterBL : IImageMasterBL
    {
        private readonly IImageMasterDAL _iImageMasterDAL;

        // Fixed base path for image storage
        private const string BaseImagePath = @"D:\Project\OFMS\OFMS_API\OFMS_API\Images\ProductImages\";

        public ImageMasterBL(IImageMasterDAL imageMasterDAL)
        {
            _iImageMasterDAL = imageMasterDAL;
        }
         
        private async Task<string> SaveImageAsync(IFormFile imageFile, int imageTypeId)
        {
            string folderName = imageTypeId switch
            {
                (int)ImageType.GROUP => "Group",
                (int)ImageType.CATEGORY => "Category",
                (int)ImageType.SUBCATEGORY => "SubCategory",
                (int)ImageType.ITEM => "Item",
                _ => "Others"
            };

            string folderPath = Path.Combine(BaseImagePath, folderName);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            string fullFilePath = Path.Combine(folderPath, uniqueFileName);

            using (var stream = new FileStream(fullFilePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return fullFilePath; // Store full path in DB
        }
         
        private void DeleteImageFile(string? imagePath)
        {
            if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
                File.Delete(imagePath);
        }
         
        public async Task<ResultMessage> AddItemMasterImage(TblImageMasterRequestTO model)
        {
            ResultMessage resultMessage = new();
            string? savedImagePath = null;
            try
            {
                savedImagePath = await SaveImageAsync(model.ImageUrl!, model.ImageTypeId);

                int id = await _iImageMasterDAL.AddItemMasterImage(model, savedImagePath);

                if (id > 0)
                {
                    resultMessage.IsSuccess = true;
                    resultMessage.Message = "Image added successfully";
                    resultMessage.StatusCode = 200;
                }
                else
                {
                    DeleteImageFile(savedImagePath);
                    resultMessage.IsSuccess = false;
                    resultMessage.Message = "Failed to add image";
                    resultMessage.StatusCode = 500;
                    resultMessage.Errors.Add("Insert operation failed");
                }
            }
            catch (Exception)
            {
                DeleteImageFile(savedImagePath);
                throw;
            }

            return resultMessage;
        }
         
        public async Task<List<tblImageMasterResponseTO>> GetListOfItemMasterImage(FilterModelTO filterModelTO)
        {
            try
            {
                return await _iImageMasterDAL.GetListOfItemMasterImage(filterModelTO);
            }
            catch (Exception)
            {
                throw;
            }
        }
         
        public async Task<tblImageMasterResponseTO> GetItemMasterImageById(int id)
        {
            try
            {
                var result = await _iImageMasterDAL.GetItemMasterImageById(id);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
         
        public async Task<ResultMessage> UpdateItemMasterImage(TblImageMasterRequestTO model)
        {
            ResultMessage resultMessage = new();
            string? newImagePath = null;
            try
            {
                var existing = await _iImageMasterDAL.GetItemMasterImageById(model.IdItemMasterImage);
                string? oldImagePath = existing.ImageUrl;

                if (model.ImageUrl != null && model.ImageUrl.Length > 0)
                    newImagePath = await SaveImageAsync(model.ImageUrl, model.ImageTypeId);
                else
                    newImagePath = oldImagePath;

                int rows = await _iImageMasterDAL.UpdateItemMasterImage(model, newImagePath);

                if (rows > 0)
                {
                    if (model.ImageUrl != null && model.ImageUrl.Length > 0)
                        DeleteImageFile(oldImagePath);

                    resultMessage.IsSuccess = true;
                    resultMessage.Message = "Image updated successfully";
                    resultMessage.StatusCode = 200;
                }
                else
                {
                    if (model.ImageUrl != null && model.ImageUrl.Length > 0)
                        DeleteImageFile(newImagePath);

                    resultMessage.IsSuccess = false;
                    resultMessage.Message = "Failed to update image";
                    resultMessage.StatusCode = 500;
                    resultMessage.Errors.Add("No record found to update or update operation failed");
                }
            }
            catch (Exception)
            {
                throw;
            }

            return resultMessage;
        }
 
        public async Task<ResultMessage> DeleteItemMasterImage(int id)
        {
            ResultMessage resultMessage = new();
            try
            {
                var existing = await _iImageMasterDAL.GetItemMasterImageById(id);
                string? imagePath = existing?.ImageUrl;

                int rows = await _iImageMasterDAL.DeleteItemMasterImage(id);

                if (rows > 0)
                {
                    DeleteImageFile(imagePath);
                    resultMessage.IsSuccess = true;
                    resultMessage.Message = "Image deleted successfully";
                    resultMessage.StatusCode = 200;
                }
                else
                {
                    resultMessage.IsSuccess = false;
                    resultMessage.Message = "Failed to delete image";
                    resultMessage.StatusCode = 500;
                    resultMessage.Errors.Add("No record found to delete");
                }
            }
            catch (Exception)
            {
                throw;
            }

            return resultMessage;
        }
    }
}
