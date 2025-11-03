using OFMS_API.Models;

namespace OFMS_API.Helper.Hub.Service
{
    public interface ICartSignalR
    {
        Task SendCartUpdate(CartSummaryDetails cartSummaryDetails);
    }
}
