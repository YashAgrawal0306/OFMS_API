using OFMS_API.Models;

namespace OFMS_API.BL.Interface
{
    public interface IOrderBL
    {
        Task<int> AddToCart(CartTO cart);
    }
}
