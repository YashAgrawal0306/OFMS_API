using Microsoft.AspNetCore.SignalR;
using OFMS_API.Helper.Hub.Service;
using OFMS_API.Models;

namespace OFMS_API.Helper.Hub
{
    public class CartSignalR:Hub<ICartSignalR>
    {
        public async Task SendCartUpdate(CartSummaryDetails cartSummaryDetails)
        {
            await Clients.Caller.SendCartUpdate(cartSummaryDetails);
        }
    }
}
