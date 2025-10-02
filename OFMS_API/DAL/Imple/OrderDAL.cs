using Dapper;
using Microsoft.Data.SqlClient;
using OFMS_API.DAL.Interface;
using OFMS_API.Models;

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
    }
}
