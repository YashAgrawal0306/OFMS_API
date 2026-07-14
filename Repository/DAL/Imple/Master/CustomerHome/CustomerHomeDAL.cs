using Dapper;
using DTO.Models.Master.CustomerHome;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Repository.DAL.Interface.Master.CustomerHome;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.DAL.Imple.Master.CustomerHome
{
    public class CustomerHomeDAL : ICustomerHomeDAL
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public CustomerHomeDAL(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        public async Task<CustomerHomeDataTO> GetCustomerHomeDataDAL()
        {
            var data = new CustomerHomeDataTO();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // 1. Get Most Ordered Dish
            string dishSql = @"
                IF EXISTS (SELECT 1 FROM tblOrderDetails)
                BEGIN
                    SELECT TOP 1 
                        i.IdItemMaster,
                        i.ItemName,
                        i.ItemDescription,
                        i.Price,
                        SUM(d.Quantity) AS TotalQtyOrdered,
                        COALESCE(img.ImageUrl, '') AS ImageUrl,
                        cat.CategoryName
                    FROM tblOrderDetails d
                    INNER JOIN tblItemMaster i ON d.IdItemMaster = i.IdItemMaster
                    LEFT JOIN tblItemMasterImage img ON i.IdItemMaster = img.ReferenceId AND img.IsMain = 1 AND img.ImageTypeId = 4
                    LEFT JOIN tblCategoryMaster cat ON i.IdCategory = cat.IdCategory
                    GROUP BY i.IdItemMaster, i.ItemName, i.ItemDescription, i.Price, img.ImageUrl, cat.CategoryName
                    ORDER BY TotalQtyOrdered DESC
                END
                ELSE
                BEGIN
                    SELECT TOP 1
                        i.IdItemMaster,
                        i.ItemName,
                        i.ItemDescription,
                        i.Price,
                        0 AS TotalQtyOrdered,
                        COALESCE(img.ImageUrl, '') AS ImageUrl,
                        cat.CategoryName
                    FROM tblItemMaster i
                    LEFT JOIN tblItemMasterImage img ON i.IdItemMaster = img.ReferenceId AND img.IsMain = 1 AND img.ImageTypeId = 4
                    LEFT JOIN tblCategoryMaster cat ON i.IdCategory = cat.IdCategory
                    WHERE i.IsActive = 1
                    ORDER BY i.IdItemMaster
                END";

            data.MostOrderedDish = (await conn.QueryAsync<MostOrderedDishTO>(dishSql)).FirstOrDefault();

            // 2. Get Most Famous Cook
            string cookSql = @"
                IF EXISTS (SELECT 1 FROM tblCookAssignment)
                BEGIN
                    SELECT TOP 1
                        u.userid AS CookUserId,
                        u.username AS CookName,
                        u.useremail AS CookEmail,
                        u.Phone_number AS CookPhone,
                        u.Profile_image AS ProfileImage,
                        COUNT(c.IdCookAssignment) AS TotalAssignedOrders
                    FROM tblCookAssignment c
                    INNER JOIN tbluser u ON c.CookUserId = u.userid
                    WHERE c.IsActive = 1
                    GROUP BY u.userid, u.username, u.useremail, u.Phone_number, u.Profile_image
                    ORDER BY TotalAssignedOrders DESC
                END
                ELSE
                BEGIN
                    SELECT TOP 1
                        u.userid AS CookUserId,
                        u.username AS CookName,
                        u.useremail AS CookEmail,
                        u.Phone_number AS CookPhone,
                        u.Profile_image AS ProfileImage,
                        0 AS TotalAssignedOrders
                    FROM tbluser u
                    INNER JOIN tblUserRoleMapping m ON u.userid = m.userid
                    WHERE m.RoleId = 3 AND u.Isactive = 1
                    ORDER BY u.userid
                END";

            data.FamousCook = (await conn.QueryAsync<FamousCookTO>(cookSql)).FirstOrDefault();

            // 3. Get Admin Details for contact
            string adminSql = @"
                SELECT TOP 1
                    u.username AS AdminName,
                    u.useremail AS AdminEmail,
                    u.Phone_number AS AdminPhone,
                    u.Profile_image AS ProfileImage
                FROM tbluser u
                INNER JOIN tblUserRoleMapping m ON u.userid = m.userid
                WHERE m.RoleId = 1 AND u.Isactive = 1
                ORDER BY u.userid";

            data.AdminContact = (await conn.QueryAsync<AdminContactTO>(adminSql)).FirstOrDefault();

            // 4. Get Promo Item 1 (Try Pizza, else 1st item)
            string promo1Sql = @"
                IF EXISTS (SELECT 1 FROM tblItemMaster WHERE iSActive = 1 AND (ItemName LIKE '%pizza%' OR ItemDescription LIKE '%pizza%'))
                BEGIN
                    SELECT TOP 1
                        i.IdItemMaster,
                        i.ItemName,
                        i.ItemDescription,
                        i.Price,
                        COALESCE(img.ImageUrl, '') AS ImageUrl
                    FROM tblItemMaster i
                    LEFT JOIN tblItemMasterImage img ON i.IdItemMaster = img.ReferenceId AND img.IsMain = 1 AND img.ImageTypeId = 4
                    WHERE i.IsActive = 1 AND (i.ItemName LIKE '%pizza%' OR i.ItemDescription LIKE '%pizza%')
                    ORDER BY i.IdItemMaster
                END
                ELSE
                BEGIN
                    SELECT TOP 1
                        i.IdItemMaster,
                        i.ItemName,
                        i.ItemDescription,
                        i.Price,
                        COALESCE(img.ImageUrl, '') AS ImageUrl
                    FROM tblItemMaster i
                    LEFT JOIN tblItemMasterImage img ON i.IdItemMaster = img.ReferenceId AND img.IsMain = 1 AND img.ImageTypeId = 4
                    WHERE i.IsActive = 1
                    ORDER BY i.IdItemMaster
                END";

            data.PromoItem1 = (await conn.QueryAsync<PromoItemTO>(promo1Sql)).FirstOrDefault();

            // 5. Get Promo Item 2 (Try Burger, else 2nd item)
            string promo2Sql = @"
                IF EXISTS (SELECT 1 FROM tblItemMaster WHERE IsActive = 1 AND (ItemName LIKE '%burger%' OR ItemDescription LIKE '%burger%'))
                BEGIN
                    SELECT TOP 1
                        i.IdItemMaster,
                        i.ItemName,
                        i.ItemDescription,
                        i.Price,
                        COALESCE(img.ImageUrl, '') AS ImageUrl
                    FROM tblItemMaster i
                    LEFT JOIN tblItemMasterImage img ON i.IdItemMaster = img.ReferenceId AND img.IsMain = 1 AND img.ImageTypeId = 4
                    WHERE i.IsActive = 1 AND (i.ItemName LIKE '%burger%' OR i.ItemDescription LIKE '%burger%')
                    ORDER BY i.IdItemMaster
                END
                ELSE
                BEGIN
                    SELECT
                        i.IdItemMaster,
                        i.ItemName,
                        i.ItemDescription,
                        i.Price,
                        COALESCE(img.ImageUrl, '') AS ImageUrl
                    FROM tblItemMaster i
                    LEFT JOIN tblItemMasterImage img ON i.IdItemMaster = img.ReferenceId AND img.IsMain = 1 AND img.ImageTypeId = 4
                    WHERE i.IsActive = 1
                    ORDER BY i.IdItemMaster OFFSET 1 ROWS FETCH NEXT 1 ROWS ONLY
                END";

            data.PromoItem2 = (await conn.QueryAsync<PromoItemTO>(promo2Sql)).FirstOrDefault();

            return data;
        }
    }
}
