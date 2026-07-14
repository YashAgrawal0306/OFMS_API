using DTO.Models.CommonModel;
using DTO.Models.CookModule;
using OFMS_API.Repository.DAL.Interface.CookModule;
using OFMS_API.Services.BL.Interface.CookModule;
using Services.BL.Interface.Notification;
using OFMS_API.DAL.Interface;
using System.Linq;
using System.Threading.Tasks;

namespace OFMS_API.Services.BL.Imple.CookModule
{
    public class CookModuleBL : ICookModuleBL
    {
        private readonly ICookModuleDAL _cookModuleDAL;
        private readonly INotificationService _notificationService;
        private readonly IuserDAL _userDAL;

        public CookModuleBL(ICookModuleDAL cookModuleDAL, INotificationService notificationService, IuserDAL userDAL)
        {
            _cookModuleDAL = cookModuleDAL;
            _notificationService = notificationService;
            _userDAL = userDAL;
        }

        public async Task<CookDashboardCountsTO> GetCookDashboardCounts(int cookUserId)
        {
            return await _cookModuleDAL.GetCookDashboardCounts(cookUserId);
        }

        public async Task<OutPutClass<CookOrderListResponseTO>> GetMyAssignedOrders(int cookUserId, FilterModelTO filter, bool completedHistory)
        {
            return await _cookModuleDAL.GetMyAssignedOrders(cookUserId, filter, completedHistory);
        }

        public async Task<CookOrderDetailResponseTO> GetOrderDetailsForCook(int cookUserId, int orderId)
        {
            return await _cookModuleDAL.GetOrderDetailsForCook(cookUserId, orderId);
        }

        public async Task<bool> AcceptOrder(int cookUserId, AcceptOrderRequestTO request)
        {
            bool success = await _cookModuleDAL.AcceptOrder(cookUserId, request);
            if (success)
            {
                await NotifyAdmins($"Cook ID {cookUserId} has accepted Order ID {request.IdOrderMaster}.", "COOK_ACCEPT");
            }
            return success;
        }

        public async Task<bool> UpdateCookingStatus(int cookUserId, UpdateCookingStatusRequestTO request)
        {
            bool success = await _cookModuleDAL.UpdateCookingStatus(cookUserId, request);
            if (success)
            {
                await NotifyAdmins($"Cook ID {cookUserId} has updated status to ID '{request.NewStatusId}' for Order ID {request.IdOrderMaster}.", "COOK_STATUS");
            }
            return success;
        }

        public async Task<bool> UpdateEstimatedTime(int cookUserId, UpdateEstimatedTimeRequestTO request)
        {
            return await _cookModuleDAL.UpdateEstimatedTime(cookUserId, request);
        }

        private async Task NotifyAdmins(string message, string notificationCode)
        {
            var admins = await _userDAL.GetAllCustomer(new FilterModelTO { RoleId = 1, PageNo = 1, PageSize = 1000 });
            var managers = await _userDAL.GetAllCustomer(new FilterModelTO { RoleId = 2, PageNo = 1, PageSize = 1000 });

            var targets = new System.Collections.Generic.List<int>();
            if (admins?.List != null) targets.AddRange(admins.List.Select(u => u.UserId));
            if (managers?.List != null) targets.AddRange(managers.List.Select(u => u.UserId));

            foreach (var userId in targets)
            {
                await _notificationService.SendNotificationAsync(userId, message, notificationCode);
            }
        }
    }
}
