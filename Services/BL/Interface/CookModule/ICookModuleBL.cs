using DTO.Models.CommonModel;
using DTO.Models.CookModule;
using System.Threading.Tasks;

namespace OFMS_API.Services.BL.Interface.CookModule
{
    public interface ICookModuleBL
    {
        Task<CookDashboardCountsTO> GetCookDashboardCounts(int cookUserId);
        Task<OutPutClass<CookOrderListResponseTO>> GetMyAssignedOrders(int cookUserId, FilterModelTO filter, bool completedHistory);
        Task<CookOrderDetailResponseTO> GetOrderDetailsForCook(int cookUserId, int orderId);
        Task<bool> AcceptOrder(int cookUserId, AcceptOrderRequestTO request);
        Task<bool> UpdateCookingStatus(int cookUserId, UpdateCookingStatusRequestTO request);
        Task<bool> UpdateEstimatedTime(int cookUserId, UpdateEstimatedTimeRequestTO request);
    }
}
