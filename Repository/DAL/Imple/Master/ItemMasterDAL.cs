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

        public Task<List<TblCategoryMasterTO>> GetListOfCategoryMaster(FilterModelTO filterModelTO)
        {
            throw new NotImplementedException();
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


        public Task<List<TblItemMasterTO>> GetListOfItemMaster(FilterModelTO filterModelTO)
        {
            throw new NotImplementedException();
        }
    }
}
