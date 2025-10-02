using OFMS_API.BL.Interface;
using OFMS_API.DAL.Interface;
using OFMS_API.Models;

namespace OFMS_API.BL.Imple
{
    public class OrderBL : IOrderBL
    {
        private readonly IOrderDAL _iOrderDAL;
        public OrderBL(IOrderDAL iOrderDAL) => _iOrderDAL = iOrderDAL;

        public Task<int> AddToCart(CartTO cart)
        {
            return _iOrderDAL.AddToCart(cart);
        }
    }
}
