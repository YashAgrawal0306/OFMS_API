using OFMS_API.Models;
using OFMS_API.Models.DTO;

namespace OFMS_API.DAL.Interface
{
    public interface IOrderDAL
    {
        Task<int> AddToCart(CartTO cart);
        Task<List<MyCartDTO>> GetMyCartItem(int userId);
        Task<int> CheckItemInCart(int userid,int menuitemid);
        Task<int> RemoveCartItem(int cartid);
        Task<CartSummaryDetails> GetCartSummaryDeatil(int userid);
        Task<int> UpdateCartItem(int cartId, int quantity);
    }
}
