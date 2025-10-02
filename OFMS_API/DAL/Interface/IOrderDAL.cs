using OFMS_API.Models;

namespace OFMS_API.DAL.Interface
{
    public interface IOrderDAL
    {
        Task<int> AddToCart(CartTO cart);
    }
}
