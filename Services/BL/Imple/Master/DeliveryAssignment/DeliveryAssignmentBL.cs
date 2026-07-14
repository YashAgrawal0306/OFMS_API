using DTO.Models.CommonModel;
using DTO.Models.Master.OrderMaster;
using Repository.DAL.Interface.Master.DeliveryAssignment;
using Services.BL.Interface.Master.DeliveryAssignment;
using Services.BL.Interface.Notification;
using OFMS_API.DAL.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Services.BL.Imple.Master.DeliveryAssignment
{
    public class DeliveryAssignmentBL : IDeliveryAssignmentBL
    {
        private readonly IDeliveryAssignmentDAL _dal;
        private readonly INotificationService _notificationService;
        private readonly IuserDAL _userDAL;
        private readonly IOrderDAL _orderDal;

        public DeliveryAssignmentBL(
            IDeliveryAssignmentDAL dal, 
            INotificationService notificationService, 
            IuserDAL userDAL,
            IOrderDAL orderDal
        )
        {
            _dal = dal;
            _notificationService = notificationService;
            _userDAL = userDAL;
            _orderDal = orderDal;
        }

        public async Task<int> AssignDeliveryBoy(CreateDeliveryAssignmentTO model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            bool orderExists = await _dal.CheckOrderExists(model.IdOrderMaster);
            if (!orderExists) throw new Exception("Order does not exist.");

            bool boyExists = await _dal.CheckDeliveryBoyExists(model.DeliveryBoyUserId);
            if (!boyExists) throw new Exception("Delivery boy does not exist.");

            bool isDuplicate = await _dal.CheckDuplicateAssignment(model.IdOrderMaster);
            if (isDuplicate) throw new Exception("Duplicate assignment not allowed. Order is already assigned.");

            int result = await _dal.AssignDeliveryBoy(model);
            if (result > 0)
            {
                // 1. Notify Delivery Boy
                await _notificationService.SendNotificationAsync(model.DeliveryBoyUserId, $"You have a new delivery assignment for Order #{model.IdOrderMaster}", "DEL_ASSIGN");

                // 2. Notify Customer
                try
                {
                    var order = await _orderDal.GetOrderMasterListByIdOrder(model.IdOrderMaster);
                    if (order != null)
                    {
                        var driverUser = await _userDAL.GetUserByIdUser(model.DeliveryBoyUserId);
                        string driverName = driverUser?.UserName ?? "a Delivery Executive";

                        string customerMsg = $"Your order #{order.OrderNo} has been dispatched. Delivery Executive {driverName} is delivering your food.";
                        await _notificationService.SendNotificationAsync(order.CustomerId, customerMsg);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error notifying customer: {ex.Message}");
                }
            }
            return result;
        }

        public async Task<DeliveryAssignmentResponseTO> GetDeliveryAssignmentById(int idDeliveryAssignment)
        {
            return await _dal.GetDeliveryAssignmentById(idDeliveryAssignment);
        }

        public async Task<OutPutClass<DeliveryAssignmentResponseTO>> GetAllDeliveryAssignments(FilterModelTO filter)
        {
            return await _dal.GetAllDeliveryAssignments(filter);
        }

        public async Task<int> AcceptDelivery(int idDeliveryAssignment, int updatedBy)
        {
            await ValidateNotDelivered(idDeliveryAssignment);
            int result = await _dal.AcceptDelivery(idDeliveryAssignment, updatedBy);
            if (result > 0) await NotifyAdmins($"Delivery Boy (User {updatedBy}) has accepted delivery assignment #{idDeliveryAssignment}.", "DEL_STATUS");
            return result;
        }

        public async Task<int> PickUpOrder(int idDeliveryAssignment, int updatedBy)
        {
            await ValidateNotDelivered(idDeliveryAssignment);
            int result = await _dal.PickUpOrder(idDeliveryAssignment, updatedBy);
            if (result > 0) await NotifyAdmins($"Delivery Boy (User {updatedBy}) has picked up order for assignment #{idDeliveryAssignment}.", "DEL_STATUS");
            return result;
        }

        public async Task<int> MarkDelivered(int idDeliveryAssignment, int updatedBy)
        {
            await ValidateNotDelivered(idDeliveryAssignment);
            int result = await _dal.MarkDelivered(idDeliveryAssignment, updatedBy);
            if (result > 0) await NotifyAdmins($"Delivery Boy (User {updatedBy}) has marked assignment #{idDeliveryAssignment} as DELIVERED.", "DEL_STATUS");
            return result;
        }

        private async Task ValidateNotDelivered(int idDeliveryAssignment)
        {
            bool isDelivered = await _dal.CheckIfDelivered(idDeliveryAssignment);
            if (isDelivered)
            {
                throw new Exception("Delivered order cannot be updated.");
            }
        }

        public async Task<DeliveryDashboardCountsTO> GetDashboardCounts(int deliveryUserId)
        {
            return await _dal.GetDashboardCounts(deliveryUserId);
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
