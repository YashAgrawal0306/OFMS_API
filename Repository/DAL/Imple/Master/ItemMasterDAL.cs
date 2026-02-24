using Dapper;
using DTO.Models.CommonModel;
using DTO.Models.Master.ItemMaster;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OFMS_API.Helper.Common;
using Repository.DAL.Interface.Master.ItemMaster;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DAL.Imple.Master
{
    public class ItemMasterDAL : IItemMasterDAL
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public ItemMasterDAL(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";
        }
        #region Group Master
        public async Task<int> AddGroupMaster(TblGroupMasterTO groupMaster)
        {
            using var connection = new SqlConnection(_connectionString);
            //int userid = Utility.GetUserId() ?? 0;
            //if (userid == 0)
            //{
            //    return 0;
            //}
            var parameters = new DynamicParameters();
            parameters.Add("@GroupName", groupMaster.GroupName, DbType.String);
            parameters.Add("@Description", groupMaster.Description, DbType.String);
            parameters.Add("@IsActive", groupMaster.IsActive, DbType.Boolean);
            parameters.Add("@CreatedOn", DateTime.UtcNow, DbType.DateTime);
            parameters.Add("@CreatedBy", groupMaster.CreatedBy, DbType.Int32);

            string sql = @"
            INSERT INTO tblGroupMaster
            (
                GroupName,
                Description,
                IsActive,
                CreatedOn,
                CreatedBy
            )
            VALUES
            (
                @GroupName,
                @Description,
                @IsActive,
                @CreatedOn,
                @CreatedBy
            );

            SELECT CAST(SCOPE_IDENTITY() AS INT);
        ";

            var result = await connection.ExecuteScalarAsync<int>(sql, parameters);

            return result;
        }

        public async Task<int> DeleteGroupMaster(int idGroup)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"DELETE FROM tblGroupMaster 
                WHERE IdGroupMaster = @Id";

            return await connection.ExecuteAsync(sql, new { Id = idGroup });
        }

        public async Task<TblGroupMasterTO> GetGroupById(int IdGroup)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT 
                    IdGroupMaster,
                    GroupName,
                    Description,
                    IsActive,
                    CreatedOn,
                    CreatedBy,
                    UpdatedOn,
                    UpdatedBy
                FROM tblGroupMaster
                WHERE IsActive = 1
                AND IdGroupMaster = @Id";

            var data = await connection.QueryFirstOrDefaultAsync<TblGroupMasterTO>(sql, new { Id = IdGroup });
            return data ?? new TblGroupMasterTO();
        }


       

        public async Task<(List<TblGroupMasterTO>, int)> GetListOfGroupMaster(FilterModelTO filter)
        {
            using var connection = new SqlConnection(_connectionString);

            int pageNo = filter.PageNo ?? 1;
            int pageSize = filter.PageSize ?? 10;
            int skip = (pageNo - 1) * pageSize;

            string sortColumn = string.IsNullOrWhiteSpace(filter.SortColumn)
                ? "CreatedOn"
                : filter.SortColumn;

            string sortOrder = string.IsNullOrWhiteSpace(filter.SortOrder)
                ? "DESC"
                : filter.SortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";

            var sql = $@"
                        SELECT  
                            IdGroupMaster,
                            GroupName,
                            Description,
                            IsActive,
                            CreatedOn,
                            CreatedBy,
                            UpdatedOn,
                            UpdatedBy
                        FROM tblGroupMaster
                        WHERE IsActive = 1
                        AND (@SearchText IS NULL OR GroupName LIKE '%' + @SearchText + '%')
                        ORDER BY {sortColumn} {sortOrder}
                        OFFSET @Skip ROWS
                        FETCH NEXT @PageSize ROWS ONLY;

                        SELECT COUNT(*)
                        FROM tblGroupMaster
                        WHERE IsActive = 1
                        AND (@SearchText IS NULL OR GroupName LIKE '%' + @SearchText + '%');
                    ";

            using var multi = await connection.QueryMultipleAsync(sql, new
            {
                SearchText = filter.SearchText,
                Skip = skip,
                PageSize = pageSize
            });

            var list = (await multi.ReadAsync<TblGroupMasterTO>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            return (list, totalCount);
        }


      
        public async Task<int> UpdateGroupMaster(TblGroupMasterTO groupMaster)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                        UPDATE tblGroupMaster
                        SET
                            GroupName   = @GroupName,
                            Description = @Description,
                            IsActive    = @IsActive,
                            UpdatedOn   = @UpdatedOn,
                            UpdatedBy   = @UpdatedBy
                        WHERE IdGroupMaster = @IdGroupMaster";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                groupMaster.GroupName,
                groupMaster.Description,
                groupMaster.IsActive,
                UpdatedOn = DateTime.Now,
                groupMaster.UpdatedBy,
                groupMaster.IdGroupMaster
            });
            return rowsAffected;
        }
        #endregion
         
        #region Category Master
        public async Task<OutPutClass<TblCategoryMasterTO>> GetListOfCategoryMaster(FilterModelTO filterModelTO)
        {
            using var conn = new SqlConnection(_connectionString);
            var output = new OutPutClass<TblCategoryMasterTO>();
            try
            {
                int pageNo = filterModelTO.PageNo ?? 1;
                int pageSize = filterModelTO.PageSize ?? 10;
                string search = filterModelTO.SearchText ?? "";
                bool isActive = filterModelTO.isActive ?? true;
                int categoryId = filterModelTO.CategoryId ?? 0;
                string flag = filterModelTO.Flag ?? "0";
                bool fetchAll = pageNo == 0 && pageSize == 0;
                int offset = fetchAll ? 0 : (pageNo - 1) * pageSize;

                string pagination = fetchAll
                    ? ""
                    : "OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";
                 
                string flagFilter = flag == "1"
                    ? "AND (ParentId IS NULL OR ParentId = 0)"
                    : flag == "2"
                        ? "AND (ParentId IS NOT NULL AND ParentId <> 0)"
                        : "";

                string query = $@"
                    SELECT IdCategory,
                           IdGroupMaster,
                           ParentId,
                           CategoryName,
                           CatDescription,
                           IsActive,
                           CreatedAt,
                           CreatedBy,
                           UpdatedAt,
                           UpdatedBy
                    FROM   TblCategoryMaster
                    WHERE  IsActive = @IsActive
                    AND    (@CategoryId = 0 OR IdCategory = @CategoryId)
                    AND    (@SearchText = '' OR CategoryName LIKE '%' + @SearchText + '%')
                    {flagFilter}
                    ORDER  BY IdCategory ASC
                    {pagination}

                    SELECT COUNT(*)
                    FROM   TblCategoryMaster
                    WHERE  IsActive = @IsActive
                    AND    (@CategoryId = 0 OR IdCategory = @CategoryId)
                    AND    (@SearchText = '' OR CategoryName LIKE '%' + @SearchText + '%')
                    {flagFilter};";

                var parameters = new DynamicParameters();
                parameters.Add("@IsActive", isActive);
                parameters.Add("@CategoryId", categoryId);
                parameters.Add("@SearchText", search);
                parameters.Add("@Offset", offset);
                parameters.Add("@PageSize", pageSize);

                var result = await conn.QueryMultipleAsync(query, parameters);
                output.List = (await result.ReadAsync<TblCategoryMasterTO>()).ToList();
                output.TotalCount = await result.ReadFirstOrDefaultAsync<int>();
                return output;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> AddCategoryMaster(TblCategoryMasterTO categoryMaster)
        {
            using var conn = new SqlConnection(_connectionString);
            try
            {
                string query = @"INSERT INTO TblCategoryMaster
                        (
                            IdGroupMaster,
                            ParentId,
                            CategoryName,
                            CatDescription,
                            IsActive,
                            CreatedAt,
                            CreatedBy
                        )
                        VALUES
                        (
                            @IdGroupMaster,
                            @ParentId,
                            @CategoryName,
                            @CatDescription,
                            @IsActive,
                            @CreatedAt,
                            @CreatedBy
                        );
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";

                var result = await conn.ExecuteScalarAsync<int>(query,
                    new
                    {
                        categoryMaster.IdGroupMaster,
                        ParentId = categoryMaster.ParentId ?? 0,
                        categoryMaster.CategoryName,
                        categoryMaster.CatDescription,
                        categoryMaster.IsActive,
                        categoryMaster.CreatedAt,
                        categoryMaster.CreatedBy
                    }
                );

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateCategoryMaster(TblCategoryMasterTO model)
        {
            using var conn = new SqlConnection(_connectionString);
            try
            {
                string query = @"
                    UPDATE TblCategoryMaster
                    SET    IdGroupMaster  = @IdGroupMaster,
                           ParentId      = @ParentId,
                           CategoryName  = @CategoryName,
                           CatDescription = @CatDescription,
                           IsActive      = @IsActive,
                           UpdatedAt     = @UpdatedAt,
                           UpdatedBy     = @UpdatedBy
                    WHERE  IdCategory    = @IdCategory";

                var parameters = new DynamicParameters();
                parameters.Add("@IdCategory", model.IdCategory);
                parameters.Add("@IdGroupMaster", model.IdGroupMaster);
                parameters.Add("@ParentId", model.ParentId);
                parameters.Add("@CategoryName", model.CategoryName);
                parameters.Add("@CatDescription", model.CatDescription);
                parameters.Add("@IsActive", model.IsActive);
                parameters.Add("@UpdatedAt", model.UpdatedAt);
                parameters.Add("@UpdatedBy", model.UpdatedBy);

                int rowsAffected = await conn.ExecuteAsync(query, parameters);

                return rowsAffected;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<TblCategoryMasterTO> GetCategoryById(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            try
            {
                string query = @"
                    SELECT IdCategory,
                           IdGroupMaster,
                           ParentId,
                           CategoryName,
                           CatDescription,
                           IsActive,
                           CreatedAt,
                           CreatedBy,
                           UpdatedAt,
                           UpdatedBy
                    FROM   TblCategoryMaster
                    WHERE  IdCategory = @IdCategory";

                var parameters = new DynamicParameters();
                parameters.Add("@IdCategory", id);

                var data = await conn.QueryFirstOrDefaultAsync<TblCategoryMasterTO>(query, parameters);
                return data ?? new TblCategoryMasterTO();
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Item Master 
        public async Task<int> AddItemMaster(TblItemMasterTO model)
        {
            using var conn = new SqlConnection(_connectionString);
            try
            {
                string query = @"
                    INSERT INTO tblItemMaster
                           (IdGroupMaster, IdCategory, IdSubCategory, ItemName, ItemDescription,
                            Price, Quantity, IsActive, CreatedAt, CreatedBy)
                    VALUES (@IdGroupMaster, @IdCategory, @IdSubCategory, @ItemName, @ItemDescription,
                            @Price, @Quantity, @IsActive, GETDATE(), @CreatedBy)";

                var parameters = new DynamicParameters();
                parameters.Add("@IdGroupMaster", model.IdGroupMaster);
                parameters.Add("@IdCategory", model.IdCategory);
                parameters.Add("@IdSubCategory", model.IdSubCategory);
                parameters.Add("@ItemName", model.ItemName);
                parameters.Add("@ItemDescription", model.ItemDescription);
                parameters.Add("@Price", model.Price);
                parameters.Add("@Quantity", model.Quantity);
                parameters.Add("@IsActive", model.IsActive);
                parameters.Add("@CreatedBy", model.CreatedBy);

                return await conn.ExecuteAsync(query, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        } 
        public async Task<OutPutClass<TblItemMasterTO>> GetListOfItemMaster(FilterModelTO filterModelTO)
        {
            using var conn = new SqlConnection(_connectionString);
            var output = new OutPutClass<TblItemMasterTO>();
            try
            {
                int pageNo = filterModelTO.PageNo ?? 1;
                int pageSize = filterModelTO.PageSize ?? 10;
                string search = filterModelTO.SearchText ?? "";
                bool isActive = filterModelTO.isActive ?? true;
                int categoryId = filterModelTO.CategoryId ?? 0;
                bool fetchAll = pageNo == 0 && pageSize == 0;
                int offset = fetchAll ? 0 : (pageNo - 1) * pageSize;

                string pagination = fetchAll
                    ? ""
                    : "OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

                string query = $@"
                    SELECT IdItemMaster,
                           IdGroupMaster,
                           IdCategory,
                           IdSubCategory,
                           ItemName,
                           ItemDescription,
                           Price,
                           Quantity,
                           IsActive,
                           CreatedAt,
                           CreatedBy,
                           UpdatedAt,
                           UpdatedBy
                    FROM   tblItemMaster
                    WHERE  IsActive    = @IsActive
                    AND    (@CategoryId = 0 OR IdCategory = @CategoryId)
                    AND    (@SearchText = '' OR ItemName LIKE '%' + @SearchText + '%')
                    ORDER  BY IdItemMaster ASC
                    {pagination}

                    SELECT COUNT(*)
                    FROM   tblItemMaster
                    WHERE  IsActive    = @IsActive
                    AND    (@CategoryId = 0 OR IdCategory = @CategoryId)
                    AND    (@SearchText = '' OR ItemName LIKE '%' + @SearchText + '%');";

                var parameters = new DynamicParameters();
                parameters.Add("@IsActive", isActive);
                parameters.Add("@CategoryId", categoryId);
                parameters.Add("@SearchText", search);
                parameters.Add("@Offset", offset);
                parameters.Add("@PageSize", pageSize);

                var result = await conn.QueryMultipleAsync(query, parameters);
                output.List = (await result.ReadAsync<TblItemMasterTO>()).ToList();
                output.TotalCount = await result.ReadFirstOrDefaultAsync<int>();
                return output;
            }
            catch (Exception)
            {
                throw;
            }
        } 
        public async Task<TblItemMasterTO> GetItemMasterById(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            try
            {
                string query = @"
                    SELECT IdItemMaster,
                           IdGroupMaster,
                           IdCategory,
                           IdSubCategory,
                           ItemName,
                           ItemDescription,
                           Price,
                           Quantity,
                           IsActive,
                           CreatedAt,
                           CreatedBy,
                           UpdatedAt,
                           UpdatedBy
                    FROM   tblItemMaster
                    WHERE  IdItemMaster = @IdItemMaster";

                var parameters = new DynamicParameters();
                parameters.Add("@IdItemMaster", id);

                var data = await conn.QueryFirstOrDefaultAsync<TblItemMasterTO>(query, parameters);
                return data ?? new TblItemMasterTO();
            }
            catch (Exception)
            {
                throw;
            }
        } 
        public async Task<int> UpdateItemMaster(TblItemMasterTO model)
        {
            using var conn = new SqlConnection(_connectionString);
            try
            {
                string query = @"
                    UPDATE tblItemMaster
                    SET    IdGroupMaster   = @IdGroupMaster,
                           IdCategory     = @IdCategory,
                           IdSubCategory  = @IdSubCategory,
                           ItemName       = @ItemName,
                           ItemDescription = @ItemDescription,
                           Price          = @Price,
                           Quantity       = @Quantity,
                           IsActive       = @IsActive,
                           UpdatedAt      = @UpdatedAt,
                           UpdatedBy      = @UpdatedBy
                    WHERE  IdItemMaster   = @IdItemMaster";

                var parameters = new DynamicParameters();
                parameters.Add("@IdItemMaster", model.IdItemMaster);
                parameters.Add("@IdGroupMaster", model.IdGroupMaster);
                parameters.Add("@IdCategory", model.IdCategory);
                parameters.Add("@IdSubCategory", model.IdSubCategory);
                parameters.Add("@ItemName", model.ItemName);
                parameters.Add("@ItemDescription", model.ItemDescription);
                parameters.Add("@Price", model.Price);
                parameters.Add("@Quantity", model.Quantity);
                parameters.Add("@IsActive", model.IsActive);
                parameters.Add("@UpdatedAt", model.UpdatedAt);
                parameters.Add("@UpdatedBy", model.UpdatedBy);

                return await conn.ExecuteAsync(query, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }
}
