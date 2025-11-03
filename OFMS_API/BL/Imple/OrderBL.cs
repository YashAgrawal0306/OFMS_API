using OFMS_API.BL.Interface;
using OFMS_API.DAL.Interface;
using OFMS_API.Models;
using OFMS_API.Models.DTO;

namespace OFMS_API.BL.Imple
{
    public class OrderBL(IOrderDAL _iOrderDAL) : IOrderBL
    {
        public async Task<int> AddToCart(CartTO cart)
        {
            //check the cart item is aleready exist or not for this user
            int userId = cart.UserId;
            int menuitemid = cart.MenuItemId;
            int Check =await _iOrderDAL.CheckItemInCart(userId, menuitemid);
            if (Check == 0) { 
            return await _iOrderDAL.AddToCart(cart);
            }
            else
            {
                return 0;
            }
        }

        public async Task<CartSummaryDetails> GetCartSummaryDeatil(int userid)
        {
            return await _iOrderDAL.GetCartSummaryDeatil(userid);
        }

        public async Task<List<MyCartDTO>> GetMyCartItem(int userId)
        {
           return await _iOrderDAL.GetMyCartItem(userId);
        }

        public async Task<int> RemoveCartItem(int cartid)
        {
            return await _iOrderDAL.RemoveCartItem(cartid);
        }

        public async Task<int> UpdateCartItem(int cartId, int quantity)
        {
           return await _iOrderDAL.UpdateCartItem(cartId, quantity);
        }
    }
}
