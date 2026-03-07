using Dapper;
using DTO.Models.CommonModel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Repository.DAL.Interface.Master.DropDownItemMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DAL.Imple.Master.DropDownItemMaster
{
    public class ItemMasterDropDownDAL : IItemMasterDropDownDAL
    {
        private readonly string _connectionString;

        public ItemMasterDropDownDAL(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }
         
        public async Task<IEnumerable<DropDownList>> GetGroupDropdown(FilterModelTO filterModelTO)
        {
            using var connection = new SqlConnection(_connectionString);

            string sortColumn = filterModelTO.SortColumn ?? "GroupName";
            string sortOrder = filterModelTO.SortOrder ?? "ASC";

            int pageNo = filterModelTO.PageNo ?? 1;
            int pageSize = filterModelTO.PageSize ?? 10;
            bool fetchAll = pageNo == 0 && pageSize == 0;
            int offset = fetchAll ? 0 : (pageNo - 1) * pageSize;

            string pagination = fetchAll
                ? ""
                : "OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            string query = $@"
                SELECT 
                    g.GroupName     AS Text,
                    g.IdGroupMaster AS Value
                FROM tblGroupMaster g
                WHERE g.IsActive = 1
                  AND (@SearchText IS NULL OR g.GroupName LIKE '%' + @SearchText + '%')
                ORDER BY g.{sortColumn} {sortOrder}
                {pagination}";

            var parameters = new DynamicParameters();
            parameters.Add("@SearchText",
                string.IsNullOrWhiteSpace(filterModelTO.SearchText) ? null : filterModelTO.SearchText);
            parameters.Add("@Offset", offset);
            parameters.Add("@PageSize", pageSize);

            return await connection.QueryAsync<DropDownList>(query, parameters);
        }

        // -------------------------------------------------------
        // Category Dropdown — filtered by Group + SearchText
        // ParentId IS NULL  →  top-level categories only
        // -------------------------------------------------------
        public async Task<IEnumerable<DropDownList>> GetCategoryDropdown(int idGroupMaster, FilterModelTO filterModelTO)
        {
            using var connection = new SqlConnection(_connectionString);

            string sortColumn = filterModelTO.SortColumn ?? "CategoryName";
            string sortOrder = filterModelTO.SortOrder ?? "ASC";

            int pageNo = filterModelTO.PageNo ?? 1;
            int pageSize = filterModelTO.PageSize ?? 10;
            bool fetchAll = pageNo == 0 && pageSize == 0;
            int offset = fetchAll ? 0 : (pageNo - 1) * pageSize;

            string pagination = fetchAll
                ? ""
                : "OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            string query = $@"
                SELECT 
                    c.CategoryName AS Text,
                    c.IdCategory   AS Value
                FROM tblCategoryMaster c
                WHERE c.IsActive      = 1
                  AND c.ParentId      = 0
                  AND c.IdGroupMaster = @IdGroupMaster
                  AND (@SearchText IS NULL OR c.CategoryName LIKE '%' + @SearchText + '%')
                ORDER BY c.{sortColumn} {sortOrder}
                {pagination}";

            var parameters = new DynamicParameters();
            parameters.Add("@IdGroupMaster", idGroupMaster);
            parameters.Add("@SearchText",
                string.IsNullOrWhiteSpace(filterModelTO.SearchText) ? null : filterModelTO.SearchText);
            parameters.Add("@Offset", offset);
            parameters.Add("@PageSize", pageSize);

            return await connection.QueryAsync<DropDownList>(query, parameters);
        }

        // -------------------------------------------------------
        // SubCategory Dropdown — ParentId = idCategory + SearchText
        // -------------------------------------------------------
        public async Task<IEnumerable<DropDownList>> GetSubCategoryDropdown(int idCategory, FilterModelTO filterModelTO)
        {
            using var connection = new SqlConnection(_connectionString);

            string sortColumn = filterModelTO.SortColumn ?? "CategoryName";
            string sortOrder = filterModelTO.SortOrder ?? "ASC";

            int pageNo = filterModelTO.PageNo ?? 1;
            int pageSize = filterModelTO.PageSize ?? 10;
            bool fetchAll = pageNo == 0 && pageSize == 0;
            int offset = fetchAll ? 0 : (pageNo - 1) * pageSize;

            string pagination = fetchAll
                ? ""
                : "OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            string query = $@"
                SELECT 
                    c.CategoryName AS Text,
                    c.IdCategory   AS Value
                FROM tblCategoryMaster c
                WHERE c.IsActive = 1
                  AND c.ParentId = @IdCategory
                  AND (@SearchText IS NULL OR c.CategoryName LIKE '%' + @SearchText + '%')
                ORDER BY c.{sortColumn} {sortOrder}
                {pagination}";

            var parameters = new DynamicParameters();
            parameters.Add("@IdCategory", idCategory);
            parameters.Add("@SearchText",
                string.IsNullOrWhiteSpace(filterModelTO.SearchText) ? null : filterModelTO.SearchText);
            parameters.Add("@Offset", offset);
            parameters.Add("@PageSize", pageSize);

            return await connection.QueryAsync<DropDownList>(query, parameters);
        }

        // -------------------------------------------------------
        // Item Dropdown — filtered by SubCategory + SearchText
        // -------------------------------------------------------
        public async Task<IEnumerable<DropDownList>> GetItemDropdown(int idSubCategory, FilterModelTO filterModelTO)
        {
            using var connection = new SqlConnection(_connectionString);

            string sortColumn = filterModelTO.SortColumn ?? "ItemName";
            string sortOrder = filterModelTO.SortOrder ?? "ASC";

            int pageNo = filterModelTO.PageNo ?? 1;
            int pageSize = filterModelTO.PageSize ?? 10;
            bool fetchAll = pageNo == 0 && pageSize == 0;
            int offset = fetchAll ? 0 : (pageNo - 1) * pageSize;

            string pagination = fetchAll
                ? ""
                : "OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            string query = $@"
                SELECT 
                    i.ItemName     AS Text,
                    i.IdItemMaster AS Value
                FROM tblItemMaster i
                WHERE i.IsActive      = 1
                  AND i.IdSubCategory = @IdSubCategory
                  AND (@SearchText IS NULL OR i.ItemName LIKE '%' + @SearchText + '%')
                ORDER BY i.{sortColumn} {sortOrder}
                {pagination}";

            var parameters = new DynamicParameters();
            parameters.Add("@IdSubCategory", idSubCategory);
            parameters.Add("@SearchText",
                string.IsNullOrWhiteSpace(filterModelTO.SearchText) ? null : filterModelTO.SearchText);
            parameters.Add("@Offset", offset);
            parameters.Add("@PageSize", pageSize);

            return await connection.QueryAsync<DropDownList>(query, parameters);
        }
    }
}
