using OFMS_API.Models;
using OFMS_API.Models.DTO;

namespace OFMS_API.BL.Interface
{
    public interface IOrderBL
    {
        Task<int> AddToCart(CartTO cart);
        Task<List<MyCartDTO>> GetMyCartItem(int userId);
    }
}
