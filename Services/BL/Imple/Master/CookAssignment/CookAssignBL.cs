using DTO.Models.CommonModel;
using DTO.Models.Master.OrderMaster;
using Repository.DAL.Interface.Master.CookAssignment;
using Services.BL.Interface.Master.CookAssignment;
using Services.BL.Interface.Notification;
using OFMS_API.DAL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.BL.Imple.Master.CookAssignment
{
    public class CookAssignBL : ICookAssignBL
    {
        private readonly ICookAssignDAL _dal;
        private readonly INotificationService _notificationService;
        private readonly IOrderDAL _orderDal;
        private readonly IuserDAL _userDal;

        public CookAssignBL(
            ICookAssignDAL dal, 
            INotificationService notificationService,
            IOrderDAL orderDal,
            IuserDAL userDal
        )
        {   
            _dal = dal;
            _notificationService = notificationService;
            _orderDal = orderDal;
            _userDal = userDal;
        }

        public async Task<int> CreateCookAssignmentBL(List<CreateCookAssignmentTO> models)
        {
            var result = await _dal.CreateCookAssignmentDAL(models);
            if (result > 0 && models != null)
            {
                foreach (var model in models)
                {
                    // 1. Notify Cook
                    await _notificationService.SendNotificationAsync(model.CookUserId, $"You have been assigned to prepare items for Order #{model.IdOrderMaster}");

                    // 2. Notify Customer
                    try
                    {
                        var order = await _orderDal.GetOrderMasterListByIdOrder(model.IdOrderMaster);
                        if (order != null)
                        {
                            var cookUser = await _userDal.GetUserByIdUser(model.CookUserId);
                            string cookName = cookUser?.UserName ?? "a Cook";

                            // Try to get item name
                            string itemName = "item";
                            var orderItem = order.orderItemResponseTO?.FirstOrDefault(i => i.IdOrderDetails == model.IdOrderDetails);
                            if (orderItem != null)
                            {
                                itemName = orderItem.ItemName;
                            }

                            string customerMsg = $"Your order #{order.OrderNo} is preparing. Cook {cookName} has been assigned to prepare your {itemName}.";
                            await _notificationService.SendNotificationAsync(order.CustomerId, customerMsg);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Silent catch to prevent workflow disruption
                        System.Diagnostics.Debug.WriteLine($"Error notifying customer: {ex.Message}");
                    }
                }
            }
            return result;
        }

        public async Task<int> UpdateKitchenStatusBL(UpdateKitchenStatusTO model)
        {
            return await _dal.UpdateKitchenStatusDAL(model);
        }

        public async Task<List<CookAssignmentResponseTO>> GetCookAssignmentListBL(FilterModelTO filterModelTO)
        {
            return await _dal.GetCookAssignmentListDAL(filterModelTO);
        }
    }
}
