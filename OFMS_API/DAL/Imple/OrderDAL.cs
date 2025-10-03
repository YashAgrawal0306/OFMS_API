using Dapper;
using Microsoft.Data.SqlClient;
using OFMS_API.DAL.Interface;
using OFMS_API.Models;
using OFMS_API.Models.DTO;

namespace OFMS_API.DAL.Imple
{
    public class OrderDAL : IOrderDAL
    {
        private string connq;
        public OrderDAL(IConfiguration configuration)
        {
            connq = configuration.GetConnectionString("DefaultConnection") ?? "";
        }
        public async Task<int> AddToCart(CartTO cart)
        {
            try
            {
                using (var con = new SqlConnection(connq))
                {
                    var query = @"
                            INSERT INTO Cart 
                            (UserId, MenuItemId, Quantity, Price, Status, CreatedAt) 
                            VALUES 
                            (@UserId, @MenuItemId, @Quantity, @Price, @Status, @CreatedAt);
            
                            SELECT CAST(SCOPE_IDENTITY() as int);";
                    cart.Status = true;
                    cart.CreatedAt = DateTime.Now;

                    var cartId = await con.QuerySingleAsync<int>(query, cart);
                    return cartId;
                }
            }
            catch(Exception)
            {
                throw;
            }
        }

        public async Task<int> CheckItemInCart(int userId, int menuItemId)
        {
            try
            {
                using var con = new SqlConnection(connq);
                var query = @"SELECT 1 FROM Cart WHERE UserId = @UserId AND MenuItemId = @MenuItemId";

                var result = await con.QueryFirstOrDefaultAsync<int>(query, new { UserId = userId, MenuItemId = menuItemId });

                return result;
            }
            catch(Exception)
            {
                throw;
            }
        }


        public Task<List<MyCartDTO>> GetMyCartItem(int userId)
        {
            try
            {
                var con = new SqlConnection(connq);
                var query = @"
                         SELECT 
                            c.CartId,
                            c.UserId,
                            c.MenuItemId,
                            mi.ProductName AS MenuItemName,
                            mi.Description AS MenuItemDescription,
                            mi.Price AS MenuItemPrice,
                            mi.ImageUrl AS MenuItemImageUrl,
                            c.Quantity,
                            c.Price AS CartPrice,
                            c.Status,
                            c.CreatedAt,
                            c.UpdatedAt
                        FROM 
                            Cart c
                        INNER JOIN 
                            menu_items mi ON c.MenuItemId = mi.MenuItemId
                        WHERE 
                            c.UserId = @userId AND c.Status = 1;";

                var cartItems = con.Query<MyCartDTO>(query, new { UserId = userId }).ToList();

                return Task.FromResult(cartItems);
            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}
