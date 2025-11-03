using Dapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using OFMS_API.BL.Imple;
using OFMS_API.DAL.Interface;
using OFMS_API.Helper.Hub;
using OFMS_API.Helper.Hub.Service;
using OFMS_API.Models;
using OFMS_API.Models.DTO;

namespace OFMS_API.DAL.Imple
{
    public class OrderDAL : IOrderDAL
    {
        private readonly string connq;
        private readonly IHubContext<CartSignalR,ICartSignalR> _hubContext;
        public OrderDAL(IConfiguration configuration, IHubContext<CartSignalR, ICartSignalR> hubContext)
        {
            connq = configuration.GetConnectionString("DefaultConnection") ?? "";
            _hubContext = hubContext;
        }
        public async Task<int> AddToCart(CartTO cart)
        {
            try
            {
                using var con = new SqlConnection(connq);
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
            catch (Exception)
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
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<CartSummaryDetails> GetCartSummaryDeatil(int userid)
        {
            using var con = new SqlConnection(connq);
            
            var query = @"
                    SELECT 
                        c.CartId,
                        c.UserId,
                        c.MenuItemId,
                        mi.ProductName AS MenuItemName,a
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

            var cartItems = (await con.QueryAsync<MyCartDTO>(query, new { userid })).ToList();

            decimal subTotal = cartItems.Sum(x => x.MenuItemPrice * x.Quantity); 
            int totalCount = cartItems.Sum(x => x.Quantity);
            decimal discount = subTotal > 1000 ? subTotal * 0.1m : 0;
            decimal deliveryCharge = subTotal > 500 ? 0 : 50;
            decimal tax = (subTotal - discount) * 0.05m;
            decimal grandTotal = subTotal - discount + deliveryCharge + tax;


            var NewItem =  new CartSummaryDetails
            {
                SubTotal = subTotal,
                CartItemCount = totalCount,
                Discount = discount,
                DeliveryCharge = deliveryCharge,
                Tax = tax,
                GrandTotal = grandTotal
            };
            await _hubContext.Clients.All.SendCartUpdate(NewItem);
            return NewItem;
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
                            c.UpdatedAt,
                            (c.Price * c.Quantity) AS TotalPrice
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

        public Task<int> RemoveCartItem(int cartid)
        {
            try
            {
                var con = new SqlConnection(connq);
                var query = @"DELETE FROM Cart WHERE CartId = @cartid";
                var result = con.ExecuteAsync(query, new { CartId = cartid });
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<int> UpdateCartItem(int cartId, int quantity)
        {
            var con = new SqlConnection(connq);
            DateTime Updatedat = DateTime.Now;
            var query = @"UPDATE Cart SET Quantity = @Quantity, UpdatedAt = @Updatedat WHERE CartId = @CartId";
            var result = con.ExecuteAsync(query, new { Quantity = quantity, UpdatedAt = DateTime.Now, CartId = cartId });
            return result;
        }
    }
}
