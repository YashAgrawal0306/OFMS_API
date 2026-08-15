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
            int result = await _dal.UpdateKitchenStatusDAL(model);

            // When a regular (non-merged) item is marked Ready (11), check if the whole order can auto-complete
            if (result > 0 && model.IdStatus == 11 && model.IdCookAssignment > 0)
            {
                try
                {
                    // Get the order id for this assignment
                    var assignment = (await _dal.GetCookAssignmentListDAL(new FilterModelTO { PageNo = 0, PageSize = 0, isActive = true }))
                        .FirstOrDefault(a => a.IdCookAssignment == model.IdCookAssignment);

                    if (assignment?.IdOrderMaster != null)
                    {
                        await _orderDal.RecalculateOrderStatusDAL(assignment.IdOrderMaster.Value);
                    }
                }
                catch (Exception ex)
                {
                    // Non-blocking — log but don't disrupt status update flow
                    System.Diagnostics.Debug.WriteLine($"RecalculateOrderStatus error: {ex.Message}");
                }
            }

            return result;
        }

        public async Task<List<CookAssignmentResponseTO>> GetCookAssignmentListBL(FilterModelTO filterModelTO)
        {
            return await _dal.GetCookAssignmentListDAL(filterModelTO);
        }

        public async Task<List<MergeableItemResponseTO>> GetMergeableCookItemsBL()
        {
            return await _dal.GetMergeableCookItemsDAL();
        }

        public async Task<int> AssignMergedCookItemBL(MergedCookAssignmentRequestTO model)
        {
            int result = await _dal.AssignMergedCookItemDAL(model);
            if (result > 0)
            {
                // Notify Cook
                await _notificationService.SendNotificationAsync(model.CookUserId, $"You have been assigned to prepare a merged batch of {model.SourceOrders.Sum(x => x.Quantity)} items.");

                // Note: Notifying individual customers is complex here because a merged item spans multiple orders.
                // Depending on requirements, we can iterate over source orders and notify them.
            }
            return result;
        }

        public async Task<int> UpdateMergedKitchenStatusBL(UpdateKitchenStatusTO model)
        {
            // 1. Update the master assignment and cascading mapping records
            List<int> affectedOrders = await _dal.UpdateMergedKitchenStatusDAL(model);

            if (affectedOrders != null && affectedOrders.Count > 0)
            {
                // 2. Recalculate each affected order independently
                foreach (var orderId in affectedOrders)
                {
                    await _orderDal.RecalculateOrderStatusDAL(orderId);
                }
            }
            
            return affectedOrders != null && affectedOrders.Count > 0 ? 1 : 0;
        }
    }
}
