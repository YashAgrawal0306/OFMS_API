using Dapper;
using DTO.Models.CommonModel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OFMS_API.DAL.Interface;
using OFMS_API.Models;
using System.Data;

namespace OFMS_API.DAL.Imple
{
    public class menuCategoryDAL : IMenuCategoryDAL
    {
        #region ctor
        private string connq;
        public menuCategoryDAL(IConfiguration configuration)
        {
            connq = configuration.GetConnectionString("DefaultConnection") ?? "";
        }
        #endregion

        #region Get

        #region GetAllCategoriesDAL
        public async Task<List<MenuCategoriesTO>> GetAllCategoriesDAL()
        {
            using var conn = new SqlConnection(connq);
            string sql = @"
                          SELECT  c.IdCategory AS id, c.CategoryName AS name, c.CatDescription AS cat_description, '' AS catImage,
                          COUNT(m.IdItemMaster) AS totalitem, CAST(MIN(m.Price)AS FLOAT) AS minprice,
                          CAST(MAX(m.Price)AS FLOAT) AS maxprice FROM tblCategoryMaster c LEFT JOIN tblItemMaster m
                          ON c.IdCategory = m.IdCategory GROUP BY c.IdCategory, c.CategoryName, c.CatDescription";

            var result = await conn.QueryAsync(sql);
            var ResultList = result.Select(x => new MenuCategoriesTO
            {
                Id = x.id,
                name = x.name ?? "",
                catImage = x.catImage ?? "",
                cat_description = x.cat_description ?? "",
                minprice = x.minprice ?? 0,
                maxprice = x.maxprice ?? 0,
                totalitem = x.totalitem ?? 0,
            });
            return ResultList.ToList();
        }
        #endregion

        #region GetAllMenuItemsListDAL
        public async Task<List<MenuItemsTO>> GetAllMenuItemsListDAL(FilterModelTO filterModelTO)
        {
            try
            {
                using var conn = new SqlConnection(connq);

                int pageSize = filterModelTO.PageSize ?? 10;
                int pageNo = filterModelTO.PageNo ?? 1;

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@CategoryId", filterModelTO.CategoryId, DbType.Int32);
                parameters.Add("@GroupId", filterModelTO.GroupId ?? 0, DbType.Int32);
                parameters.Add("@SubCategoryId", filterModelTO.SubCategoryId ?? 0, DbType.Int32);
                parameters.Add("@ItemId", filterModelTO.ItemId ?? 0, DbType.Int32);
                parameters.Add("@FromDate", filterModelTO.FromDate, DbType.DateTime);
                parameters.Add("@ToDate", filterModelTO.ToDate, DbType.DateTime);
                parameters.Add("@SearchText", string.IsNullOrWhiteSpace(filterModelTO.SearchText) ? null : $"%{filterModelTO.SearchText}%", DbType.String);
                parameters.Add("@IsActive", filterModelTO.isActive, DbType.Boolean);
                parameters.Add("@PageSize", pageSize, DbType.Int32);
                parameters.Add("@PageNo", pageNo, DbType.Int32);

                var sql = @"
                    ;WITH Paginated AS (
                        SELECT 
                            m.IdItemMaster AS MenuItemId,
                            m.ItemName AS MenuName,
                            m.ItemName AS ProductName,
                            m.IdCategory AS CategoryId,
                            c.CategoryName AS CategoryName,
                            g.GroupName AS GroupName,
                            sub.CategoryName AS SubCategoryName,
                            m.IsActive AS Status,
                            m.Price,
                            m.Price AS FinalPrice,
                            0 AS DiscountPercent,
                            m.Ingredients,
                            m.ItemDescription AS Description,
                            0 AS CookingTimeMinutes,    
                            COALESCE(img.ImageUrl, '') AS ImageUrl,
                            COALESCE(img.ImageUrl, '') AS ThumbnailUrl,
                            m.CreatedAt,
                            m.UpdatedAt,
                            ROW_NUMBER() OVER (ORDER BY m.IdItemMaster ASC) AS RowNum
                        FROM tblItemMaster m
                        INNER JOIN tblCategoryMaster c ON m.IdCategory = c.IdCategory
                        LEFT JOIN tblGroupMaster g ON m.IdGroupMaster = g.IdGroupMaster
                        LEFT JOIN tblCategoryMaster sub ON m.IdSubCategory = sub.IdCategory
                        LEFT JOIN tblItemMasterImage img ON m.IdItemMaster = img.ReferenceId AND img.IsMain = 1 AND img.ImageTypeId = 4
                        WHERE (@SearchText IS NULL OR @SearchText  = ''
                               OR (m.ItemName LIKE @SearchText 
                                   OR c.CategoryName LIKE @SearchText 
                                   OR m.ItemDescription LIKE @SearchText 
                                   OR m.Ingredients LIKE @SearchText))
                          AND (@IsActive IS NULL OR m.IsActive = @IsActive)
                          AND (@CategoryId = 0 OR m.IdCategory = @CategoryId)
                          AND (@GroupId = 0 OR m.IdGroupMaster = @GroupId)
                          AND (@SubCategoryId = 0 OR m.IdSubCategory = @SubCategoryId)
                          AND (@ItemId = 0 OR m.IdItemMaster = @ItemId)
                          AND (@FromDate IS NULL OR m.CreatedAt >= @FromDate)
                          AND (@ToDate IS NULL OR m.CreatedAt <= @ToDate)
                    )
                    SELECT *
                    FROM Paginated
                    WHERE RowNum BETWEEN ((@PageNo - 1) * @PageSize + 1) AND (@PageNo * @PageSize);
                    ";

                var result = await conn.QueryAsync(sql, parameters);

                var resultlist = result.Select(x => new MenuItemsTO
                {
                    MenuItemId = x.MenuItemId ?? 0,
                    MenuName = x.MenuName ?? "",
                    ProductName = x.ProductName ?? "",
                    CategoryId = x.CategoryId ?? 0,
                    CategoryName = x.CategoryName ?? "",
                    GroupName = x.GroupName ?? "",
                    SubCategoryName = x.SubCategoryName ?? "",
                    Status = x.Status ?? false,
                    Price = x.Price ?? 0,
                    FinalPrice = x.FinalPrice ?? 0,
                    DiscountPercent = x.DiscountPercent ?? 0,
                    Ingredients = x.Ingredients ?? "",
                    Description = x.Description ?? "",
                    CookingTimeMinutes = x.CookingTimeMinutes ?? null,
                    ImageUrl = x.ImageUrl ?? "",
                    ThumbnailUrl = x.ThumbnailUrl ?? "",
                    CreatedAt = x.CreatedAt ?? DateTime.MinValue,
                    UpdatedAt = x.UpdatedAt ?? DateTime.MinValue
                }).ToList();

                return resultlist;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region GetCategoryDropDownListDAL
        public async Task<List<DropDownList>> GetCategoryDropDownListDAL()
        {
            var con = new SqlConnection(connq);
            var query = "SELECT CategoryName AS Text, IdCategory AS Value FROM tblCategoryMaster WHERE IsActive = 1";
            var result = await con.QueryAsync<DropDownList>(query);
            return result.ToList();
        }
        #endregion

        #endregion

        #region Post

        #region AddNewCategory
        public async Task<int> AddNewCategory(MenuCategoriesTO categories)
        {
            using var conn = new SqlConnection(connq);
            var parameter = new DynamicParameters();
            parameter.Add("@Name", categories.name, DbType.String);
            parameter.Add("@cat_description", categories.cat_description, DbType.String);
            string sql = @"INSERT INTO tblCategoryMaster (CategoryName, CatDescription, IsActive, CreatedAt) VALUES (@Name, @cat_description, 1, GETDATE())";
            return await conn.ExecuteAsync(sql, parameter);
        }
        #endregion

        #region AddNewMenuItem
        public async Task<int> AddNewMenuItem(MenuItemsTO menu_Item)
        {
            using var conn = new SqlConnection(connq);
            var parameter = new DynamicParameters();
            parameter.Add("@ItemName", menu_Item.ProductName ?? menu_Item.MenuName, DbType.String);
            parameter.Add("@IdCategory", menu_Item.CategoryId, DbType.Int32);
            parameter.Add("@IsActive", menu_Item.Status, DbType.Boolean);
            parameter.Add("@Price", menu_Item.Price, DbType.Decimal);
            parameter.Add("@Ingredients", menu_Item.Ingredients, DbType.String);
            parameter.Add("@ItemDescription", menu_Item.Description, DbType.String);
            parameter.Add("@CreatedAt", DateTime.Now, DbType.DateTime);
            parameter.Add("@UpdatedAt", DateTime.Now, DbType.DateTime);

            string query = @"INSERT INTO tblItemMaster
                    (ItemName, IdCategory, IsActive, Price, Ingredients, ItemDescription, CreatedAt, UpdatedAt)
                    VALUES
                    (@ItemName, @IdCategory, @IsActive, @Price, @Ingredients, @ItemDescription, @CreatedAt, @UpdatedAt);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

            int newId = await conn.ExecuteScalarAsync<int>(query, parameter);

            if (newId > 0 && !string.IsNullOrEmpty(menu_Item.ImageUrl))
            {
                var imgParam = new DynamicParameters();
                imgParam.Add("@ReferenceId", newId, DbType.Int32);
                imgParam.Add("@ImageUrl", menu_Item.ImageUrl, DbType.String);
                string imgQuery = "INSERT INTO tblItemMasterImage (ReferenceId, ImageUrl, IsMain, CreatedAt) VALUES (@ReferenceId, @ImageUrl, 1, GETDATE())";
                await conn.ExecuteAsync(imgQuery, imgParam);
            }

            return newId;
        }
        #endregion

        #region AddDublicateMenuItemDAL
        public async Task<int> AddDublicateMenuItemDAL(CopyDublicateItemTO itemTO)
        {
            using var conn = new SqlConnection(connq);
            try
            {
                int menuId = itemTO.menuItemId;
                string newName = itemTO.ProductName ?? "";
                string columns = "ItemName,IdCategory,IsActive";
                string selectColumns = "@ProductName,IdCategory,IsActive";

                if (itemTO.CopyPricingInfo == true)
                {
                    columns += ",Price";
                    selectColumns += ",Price";
                }
                else
                {
                    columns += ",Price";
                    selectColumns += ",0";
                }
                if (itemTO.Copyingredients == true)
                {
                    columns += ",Ingredients";
                    selectColumns += ",Ingredients";
                }

                columns += ",ItemDescription,CreatedAt,UpdatedAt";
                selectColumns += ",ItemDescription,GETDATE(),GETDATE()";

                string insertquery = $@"
                                    INSERT INTO tblItemMaster ({columns})
                                    SELECT {selectColumns}
                                    FROM tblItemMaster
                                    WHERE IdItemMaster = @MenuId;
                                    SELECT CAST(SCOPE_IDENTITY() as int);";

                int newId = await conn.ExecuteScalarAsync<int>(insertquery, new { MenuId = menuId, ProductName = newName });

                if (newId > 0)
                {
                    string copyImgQuery = @"
                        INSERT INTO tblItemMasterImage (ReferenceId, ImageUrl, IsMain, CreatedAt)
                        SELECT @NewId, ImageUrl, IsMain, GETDATE()
                        FROM tblItemMasterImage
                        WHERE ReferenceId = @MenuId AND IsMain = 1";
                    await conn.ExecuteAsync(copyImgQuery, new { NewId = newId, MenuId = menuId });
                }

                return newId > 0 ? 1 : 0;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #endregion

        #region Edit

        #region EditMenuItemDAL
        public async Task<int> EditMenuItemDAL(MenuItemsTO menu_Item)
        {
            using var conn = new SqlConnection(connq);
            var parameter = new DynamicParameters();
            parameter.Add("@IdItemMaster", menu_Item.MenuItemId, DbType.Int32);
            parameter.Add("@ItemName", menu_Item.ProductName ?? menu_Item.MenuName, DbType.String);
            parameter.Add("@IdCategory", menu_Item.CategoryId, DbType.Int32);
            parameter.Add("@IsActive", menu_Item.Status, DbType.Boolean);
            parameter.Add("@Price", menu_Item.Price, DbType.Decimal);
            parameter.Add("@Ingredients", menu_Item.Ingredients, DbType.String);
            parameter.Add("@ItemDescription", menu_Item.Description, DbType.String);
            parameter.Add("@UpdatedAt", DateTime.Now, DbType.DateTime);

            var query = @" UPDATE tblItemMaster SET ItemName = @ItemName, IdCategory = @IdCategory,
                        IsActive = @IsActive, Price = @Price, Ingredients = @Ingredients, 
                        ItemDescription = @ItemDescription, UpdatedAt = @UpdatedAt WHERE IdItemMaster = @IdItemMaster; ";

            var rowsAffected = await conn.ExecuteAsync(query, parameter);

            if (rowsAffected > 0 && !string.IsNullOrEmpty(menu_Item.ImageUrl))
            {
                string checkImgQuery = "SELECT COUNT(1) FROM tblItemMasterImage WHERE ReferenceId = @IdItemMaster AND IsMain = 1";
                int imgCount = await conn.ExecuteScalarAsync<int>(checkImgQuery, new { IdItemMaster = menu_Item.MenuItemId });
                if (imgCount > 0)
                {
                    string updateImgQuery = "UPDATE tblItemMasterImage SET ImageUrl = @ImageUrl, UpdatedOn = GETDATE() WHERE ReferenceId = @IdItemMaster AND IsMain = 1";
                    await conn.ExecuteAsync(updateImgQuery, new { IdItemMaster = menu_Item.MenuItemId, ImageUrl = menu_Item.ImageUrl });
                }
                else
                {
                    string insertImgQuery = "INSERT INTO tblItemMasterImage (ReferenceId, ImageUrl, IsMain, CreatedAt) VALUES (@IdItemMaster, @ImageUrl, 1, GETDATE())";
                    await conn.ExecuteAsync(insertImgQuery, new { IdItemMaster = menu_Item.MenuItemId, ImageUrl = menu_Item.ImageUrl });
                }
            }

            return rowsAffected;
        }
        #endregion

        #endregion

        #region Delete

        #region DeleteMenuItem
        public async Task<int> DeleteMenuItemDAL(int menuid)
        {
            using var conn = new SqlConnection(connq);
            string deleteImg = "DELETE FROM tblItemMasterImage WHERE ReferenceId = @MenuId";
            await conn.ExecuteAsync(deleteImg, new { MenuId = menuid });

            var sqlquery = "DELETE FROM tblItemMaster WHERE IdItemMaster = @MenuId";
            int result = await conn.ExecuteAsync(sqlquery, new { MenuId = menuid });
            return result;
        }
        #endregion

        #endregion
    }
}
