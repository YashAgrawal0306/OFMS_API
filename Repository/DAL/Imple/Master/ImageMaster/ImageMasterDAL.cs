using Dapper;
using DTO.Models.CommonModel;
using DTO.Models.Master.ImageMaster;
using DTO.Models.Master.ImageMaster.ResponseModel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Repository.DAL.Interface.Master.ImageMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DAL.Imple.Master.ImageMaster
{
    public class ImageMasterDAL:IImageMasterDAL
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        public ImageMasterDAL(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        public async Task<int> AddItemMasterImage(TblImageMasterRequestTO model, string imageUrl)
        {
            using var conn = new SqlConnection(_connectionString);
            try
            {
                string query = @"
                    INSERT INTO tblItemMasterImage
                           (ImageTypeId, ReferenceId, ImageUrl, IsMain, DisplayOrder, CreatedAt, CreatedBy)
                    VALUES (@ImageTypeId, @ReferenceId, @ImageUrl, @IsMain, @DisplayOrder, GETDATE(), @CreatedBy)";

                var parameters = new DynamicParameters();
                parameters.Add("@ImageTypeId", model.ImageTypeId);
                parameters.Add("@ReferenceId", model.ReferenceId);
                parameters.Add("@ImageUrl", imageUrl);
                parameters.Add("@IsMain", model.IsMain);
                parameters.Add("@DisplayOrder", model.DisplayOrder);
                parameters.Add("@CreatedBy", model.CreatedBy);

                return await conn.ExecuteAsync(query, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<tblImageMasterResponseTO>> GetListOfItemMasterImage(FilterModelTO filterModelTO)
        {
            using var conn = new SqlConnection(_connectionString);
            try
            {
                int pageNo = filterModelTO.PageNo ?? 1;
                int pageSize = filterModelTO.PageSize ?? 10;
                string search = filterModelTO.SearchText ?? "";
                int referenceId = filterModelTO.CategoryId ?? 0;
                int imageTypeId = int.TryParse(filterModelTO.Flag, out int ft) ? ft : 0;
                bool fetchAll = pageNo == 0 && pageSize == 0;
                int offset = fetchAll ? 0 : (pageNo - 1) * pageSize;

                string pagination = fetchAll
                    ? ""
                    : "OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

                string query = $@"
                    SELECT IdItemMasterImage,
                           ImageTypeId,
                           ReferenceId,
                           ImageUrl,
                           IsMain,
                           DisplayOrder,
                           CreatedAt,
                           CreatedBy,
                           UpdatedBy,
                           UpdatedOn
                    FROM   tblItemMasterImage
                    WHERE  (@ImageTypeId = 0 OR ImageTypeId = @ImageTypeId)
                    AND    (@ReferenceId = 0 OR ReferenceId = @ReferenceId)
                    AND    (@SearchText  = '' OR ImageUrl LIKE '%' + @SearchText + '%')
                    ORDER  BY DisplayOrder ASC, IdItemMasterImage ASC
                    {pagination}";

                var parameters = new DynamicParameters();
                parameters.Add("@ImageTypeId", imageTypeId);
                parameters.Add("@ReferenceId", referenceId);
                parameters.Add("@SearchText", search);
                parameters.Add("@Offset", offset);
                parameters.Add("@PageSize", pageSize);

                var result = await conn.QueryAsync<tblImageMasterResponseTO>(query, parameters);
                return result.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<tblImageMasterResponseTO> GetItemMasterImageById(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            try
            {
                string query = @"
                    SELECT IdItemMasterImage,
                           ImageTypeId,
                           ReferenceId,
                           ImageUrl,
                           IsMain,
                           DisplayOrder,
                           CreatedAt,
                           CreatedBy,
                           UpdatedBy,
                           UpdatedOn
                    FROM   tblItemMasterImage
                    WHERE  IdItemMasterImage = @IdItemMasterImage";

                var parameters = new DynamicParameters();
                parameters.Add("@IdItemMasterImage", id);

                return await conn.QueryFirstOrDefaultAsync<tblImageMasterResponseTO>(query, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<AttachmentListTO>> GetItemMasterImageByReferenceId(int IdReference, int ImageTypeId)
        {
            using var conn = new SqlConnection(_connectionString);
            try
            {
                string query = @"
                   SELECT IdItemMasterImage,
                     ImageTypeId,
                     ReferenceId,
                     ImageUrl,
                     IsMain,
                     DisplayOrder 
                  FROM   tblItemMasterImage
                  WHERE ReferenceId = @ReferenceId AND ImageTypeId = @ImageTypeId";

                var parameters = new DynamicParameters();
                parameters.Add("@ReferenceId", IdReference);
                parameters.Add("@ImageTypeId", IdReference);

                var data = await conn.QueryAsync<AttachmentListTO>(query, parameters);
                return data.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateItemMasterImage(TblImageMasterRequestTO model, string imageUrl)
        {
            using var conn = new SqlConnection(_connectionString);
            try
            {
                string query = @"
                    UPDATE tblItemMasterImage
                    SET    ImageTypeId  = @ImageTypeId,
                           ReferenceId  = @ReferenceId,
                           ImageUrl     = @ImageUrl,
                           IsMain       = @IsMain,
                           DisplayOrder = @DisplayOrder,
                           UpdatedBy    = @UpdatedBy,
                           UpdatedOn    = @UpdatedOn
                    WHERE  IdItemMasterImage = @IdItemMasterImage";

                var parameters = new DynamicParameters();
                parameters.Add("@IdItemMasterImage", model.IdItemMasterImage);
                parameters.Add("@ImageTypeId", model.ImageTypeId);
                parameters.Add("@ReferenceId", model.ReferenceId);
                parameters.Add("@ImageUrl", imageUrl);
                parameters.Add("@IsMain", model.IsMain);
                parameters.Add("@DisplayOrder", model.DisplayOrder);
                parameters.Add("@UpdatedBy", model.UpdatedBy);
                parameters.Add("@UpdatedOn", model.UpdatedOn);

                return await conn.ExecuteAsync(query, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
         

        public async Task<int> DeleteItemMasterImage(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            try
            {
                string query = @"
                    DELETE FROM tblItemMasterImage
                    WHERE IdItemMasterImage = @IdItemMasterImage";

                var parameters = new DynamicParameters();
                parameters.Add("@IdItemMasterImage", id);

                return await conn.ExecuteAsync(query, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
