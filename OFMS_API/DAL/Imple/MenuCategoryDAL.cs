using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OFMS_API.BL.Imple;
using OFMS_API.DAL.Interface;
using OFMS_API.MinioS3;
using OFMS_API.Models;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

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
                          SELECT  c.id, c.name, c.cat_description,c.catImage,
                          COUNT(m.MenuItemId) AS totalitem, CAST(MIN(m.Price)AS FLOAT) AS minprice,
                          CAST(MAX(m.Price)AS FLOAT) AS maxprice FROM menu_categories c LEFT JOIN menu_items m
                          ON c.id = m.CategoryId GROUP BY c.id, c.name, c.cat_description ,c.catImage";

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
            if (result != null)
            {
                return ResultList.ToList();
            }
            else
            {
                return ResultList?.ToList() ?? new List<MenuCategoriesTO>();
            }
        }
        #endregion

        #region GetAllMenuItemsListDAL
        public async Task<List<MenuItemsTO>> GetAllMenuItemsListDAL(FilterModelTO filterModelTO)
        {
            try
            {
                using var conn = new SqlConnection(connq);

                // Apply safe defaults
                int pageSize = filterModelTO.PageSize ?? 0;
                int pageNo = filterModelTO.PageNo?? 0;

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@CategoryId", filterModelTO.CategoryId, DbType.Int32);
                parameters.Add("@SearchText", filterModelTO.SearchText, DbType.String);
                parameters.Add("@IsActive", filterModelTO.isActive, DbType.Boolean);
                parameters.Add("@PageSize", pageSize, DbType.Int32);
                parameters.Add("@PageNo", pageNo, DbType.Int32);

                var sql = @"
            SELECT m.*, c.id AS CategoryId, c.name AS CategoryName
            FROM menu_items m
            INNER JOIN menu_categories c ON m.CategoryId = c.id
            WHERE (@SearchText IS NULL OR @SearchText = m.ProductName OR @SearchText = m.MenuName)
              AND (@IsActive IS NULL OR @IsActive = m.Status)
            ORDER BY " + filterModelTO.SortColumn + " " + filterModelTO.SortOrder + @"
            OFFSET (@PageNo - 1) * @PageSize ROWS
            FETCH NEXT @PageSize ROWS ONLY;";

                var result = await conn.QueryAsync(sql, parameters);

                var resultlist = result.Select(x => new MenuItemsTO
                {
                    MenuItemId = x.MenuItemId ?? 0,
                    MenuName = x.MenuName ?? "",
                    ProductName = x.ProductName ?? "",
                    CategoryId = x.CategoryId ?? 0,
                    CategoryName = x.CategoryName ?? "",
                    Status = x.Status ?? 0,
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

        #region
        public async Task<List<DropDownList>> GetCategoryDropDownListDAL()
        {
            var con = new SqlConnection(connq);
            var query = "Select name as Text,id as Value from menu_categories";
            var result = await con.QueryAsync<DropDownList>(query);
            var resultlist = result.Select(raw => new DropDownList
            {
                Text = raw.Text,
                Value = raw.Value
            });

            return resultlist.ToList();
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
            parameter.Add("@catImage", categories.catImage, DbType.String);
            parameter.Add("@cat_description", categories.cat_description, DbType.String);
            string sql = @" INSERT INTO menu_categories (name,catImage,cat_description) VALUES (@Name,@catImage,@cat_description)";
            return await conn.ExecuteAsync(sql, parameter);
        }

        #endregion

        #region AddNewMenuItem
        public async Task<int> AddNewMenuItem(MenuItemsTO menu_Item)
        {
            using var conn = new SqlConnection(connq);
            var parameter = new DynamicParameters();
            parameter.Add("@MenuName", menu_Item.MenuName, DbType.String);
            parameter.Add("@ProductName", menu_Item.ProductName, DbType.String);
            parameter.Add("@CategoryId", menu_Item.CategoryId, DbType.Int32);
            parameter.Add("@Status", menu_Item.Status, DbType.Boolean);
            parameter.Add("@Price", menu_Item.Price, DbType.Decimal);
            parameter.Add("@DiscountPercent", menu_Item.DiscountPercent, DbType.Decimal);
            parameter.Add("@Ingredients", menu_Item.Ingredients, DbType.String);
            parameter.Add("@Description", menu_Item.Description, DbType.String);
            parameter.Add("@CookingTimeMinutes", menu_Item.CookingTimeMinutes, DbType.Int32);
            parameter.Add("@ImageUrl", menu_Item.ImageUrl, DbType.String);
            parameter.Add("@ThumbnailUrl", menu_Item.ImageUrl, DbType.String);
            parameter.Add("@CreatedAt", DateTime.Now, DbType.DateTime);
            parameter.Add("@UpdatedAt", DateTime.Now, DbType.DateTime);

            string query = @"INSERT INTO menu_items
                    (MenuName, ProductName, CategoryId, Status, Price, DiscountPercent,
                    Ingredients, Description, CookingTimeMinutes, ImageUrl, ThumbnailUrl, CreatedAt, UpdatedAt)
                    VALUES
                    (@MenuName, @ProductName, @CategoryId, @Status, @Price, @DiscountPercent,
                    @Ingredients, @Description, @CookingTimeMinutes, @ImageUrl, @ThumbnailUrl, @CreatedAt, @UpdatedAt);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

            int newId = await conn.ExecuteScalarAsync<int>(query, parameter);
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
                string columns = "MenuName,ProductName,CategoryId,Status";
                string selectColumns = "MenuName, @ProductName, CategoryId, Status";

                if (itemTO.CopyPricingInfo == true)
                {
                    columns += ",Price,DiscountPercent";
                    selectColumns += ",Price,DiscountPercent";
                }
                else
                {
                    columns += ",Price,DiscountPercent";
                    selectColumns += ",0,0";
                }
                if (itemTO.Copyingredients == true)
                {
                    columns += ",Ingredients";
                    selectColumns += ",Ingredients";
                }

                columns += ",Description,CookingTimeMinutes,ImageUrl,ThumbnailUrl,CreatedAt,UpdatedAt";
                selectColumns += ",Description,CookingTimeMinutes,ImageUrl,ThumbnailUrl,CreatedAt,UpdatedAt";

                string insertquery = $@"
                                    INSERT INTO menu_items ({columns})
                                    SELECT {selectColumns}
                                    FROM menu_items
                                    WHERE MenuItemId = @MenuId";

                var result = await conn.ExecuteAsync(insertquery, new { MenuId = menuId, ProductName = newName });
                return result;

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
            parameter.Add("@MenuItemId", menu_Item.MenuItemId, DbType.Int32); // required!
            parameter.Add("@MenuName", menu_Item.MenuName, DbType.String);
            parameter.Add("@ProductName", menu_Item.ProductName, DbType.String);
            parameter.Add("@CategoryId", menu_Item.CategoryId, DbType.Int32);
            parameter.Add("@Status", menu_Item.Status, DbType.Boolean);
            parameter.Add("@Price", menu_Item.Price, DbType.Decimal);
            parameter.Add("@DiscountPercent", menu_Item.DiscountPercent, DbType.Decimal);
            parameter.Add("@Ingredients", menu_Item.Ingredients, DbType.String);
            parameter.Add("@Description", menu_Item.Description, DbType.String);
            parameter.Add("@CookingTimeMinutes", menu_Item.CookingTimeMinutes, DbType.Int32);
            parameter.Add("@ImageUrl", menu_Item.ImageUrl, DbType.String);
            parameter.Add("@ThumbnailUrl", menu_Item.ThumbnailUrl, DbType.String);
            parameter.Add("@UpdatedAt", DateTime.Now, DbType.DateTime);

            var query = @" UPDATE menu_items SET MenuName = @MenuName, ProductName = @ProductName, CategoryId = @CategoryId,
                        Status = @Status, Price = @Price, DiscountPercent = @DiscountPercent, Ingredients = @Ingredients, 
                        Description = @Description, CookingTimeMinutes = @CookingTimeMinutes, ImageUrl = @ImageUrl, ThumbnailUrl = @ThumbnailUrl,
                        UpdatedAt = @UpdatedAt WHERE MenuItemId = @MenuItemId; ";

            var rowsAffected = await conn.ExecuteAsync(query, parameter);
            return rowsAffected;
        }


        #endregion

        #endregion

        #region Delete

        #region DeleteMenuItem
        public async Task<int> DeleteMenuItemDAL(int menuid)
        {
            using var conn = new SqlConnection(connq);

            var sqlquery = "DELETE FROM menu_items WHERE MenuItemId = @MenuItemId";
            int result = await conn.ExecuteAsync(sqlquery, new { MenuItemId = menuid });
            return result;
        }

        #endregion

        #endregion
    }
}
