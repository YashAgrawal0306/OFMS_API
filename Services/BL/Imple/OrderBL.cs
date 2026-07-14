using DTO.Models.CommonModel;
using DTO.Models.Master.OrderMaster;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OFMS_API.BL.Interface;
using OFMS_API.DAL.Imple;
using OFMS_API.DAL.Interface;
using OFMS_API.Models;
using OFMS_API.Models.DTO;
using Services.BL.Interface.Notification;
namespace OFMS_API.BL.Imple
{
    public class OrderBL : IOrderBL
    {
        private readonly string connq;
        private readonly IOrderDAL _iOrderDAL;
        private readonly IuserDAL _iuserDAL;
        private readonly INotificationService _notificationService;
        private readonly ICartRepository _cartRepo;

        public OrderBL(IOrderDAL iOrderDAL, IuserDAL iuserDAL, INotificationService notificationService, IConfiguration configuration, ICartRepository cartRepo)
        {
            _iOrderDAL = iOrderDAL;
            _iuserDAL = iuserDAL;
            _notificationService = notificationService;
            connq = configuration.GetConnectionString("DefaultConnection") ?? "";
            _cartRepo = cartRepo;
        }
        public async Task<ResultMessage> AddOrderMaster(OrderMasterTO orderMasterTO)
        {
            ResultMessage resultMessage = new();
            using var conn = new SqlConnection(connq);
            await conn.OpenAsync();
            SqlTransaction tran = conn.BeginTransaction();
            int result = await _iOrderDAL.AddOrderMaster(orderMasterTO,conn,tran);
            if (result > 0)
            {
                orderMasterTO.PaymentDetail.IdOrderMaster = result;
                int idPaymentDeatil =await _iOrderDAL.AddPaymentData(orderMasterTO.PaymentDetail,conn,tran);
                if (idPaymentDeatil > 0)
                {
                    // Clear the cart inside the same transaction
                    await _cartRepo.ClearCartWithConnectionAsync(orderMasterTO.CustomerId, conn, tran);

                    tran.Commit();
                    resultMessage.IsSuccess = true;
                    resultMessage.Message = "Order Added Successfully";

                    // Notify Admin (RoleId=1) and Manager (RoleId=2)
                    var admins = await _iuserDAL.GetAllCustomer(new FilterModelTO { RoleId = 1, PageNo = 1, PageSize = 1000 });
                    var managers = await _iuserDAL.GetAllCustomer(new FilterModelTO { RoleId = 2, PageNo = 1, PageSize = 1000 });

                    var adminAndManagers = new List<int>();
                    if (admins?.List != null) adminAndManagers.AddRange(admins.List.Select(u => u.UserId));
                    if (managers?.List != null) adminAndManagers.AddRange(managers.List.Select(u => u.UserId));

                    foreach (var userId in adminAndManagers)
                    {
                        await _notificationService.SendNotificationAsync(userId, $"New Order Placed: {orderMasterTO.OrderNo}", "NEW_ORDER");
                    }
                }
                else
                {
                    tran.Rollback();
                    resultMessage.IsSuccess = false;
                    resultMessage.Message = "Payment Detail Not Added";
                }
            }
            else
            {
                tran.Rollback();
                resultMessage.IsSuccess = false;
                resultMessage.Message = "Order Not Added";
            }
            return resultMessage;

        }

        public async Task<OutPutClass<OrderListResponseTO>> GetOrderMasterList(OrderListFilter orderListFilter)
        {
            var data = await _iOrderDAL.GetOrderMasterList(orderListFilter);
            if (data != null)
            {
                foreach(var item in data.List)
                {
                    if (item.IdAddressMapping > 0)
                    {
                        item.tblAddressResponseTO = await _iOrderDAL.GetAddressByIdAddressMapping(item.IdAddressMapping);
                    }
                }
            }
            return data;
        }
        public async Task<OrderListResponseTO> GetOrderMasterListByIdOrder(int IdOrderMaster)
        {
            var data =  await _iOrderDAL.GetOrderMasterListByIdOrder(IdOrderMaster);
            if(data != null)
            {
                data.tblAddressResponseTO = await _iOrderDAL.GetAddressByIdAddressMapping(data.IdAddressMapping);
            }
            return data;
        }

        public async Task<bool> UpdateOrderMaster(OrderMasterTO order)
        {
            return await _iOrderDAL.UpdateOrderMaster(order);
        }

        public async Task<ResultMessage> UpdateOrderStatus(UpdateOrderStatusRequest request)
        {
            var result = await _iOrderDAL.UpdateOrderStatus(request);
            if (result.IsSuccess)
            {
                var order = await GetOrderMasterListByIdOrder(request.IdOrderMaster);
                if (order != null)
                {
                    string statusMsg = request.IdStatus switch
                    {
                        1 => "placed successfully",
                        2 => "accepted by Admin",
                        3 => "assigned to a cook",
                        4 => "prepared and is ready for pickup",
                        5 => "assigned to a delivery executive",
                        6 => "delivered successfully",
                        7 => "cancelled",
                        _ => "updated"
                    };

                    await _notificationService.SendNotificationAsync(order.CustomerId, $"Your order #{order.OrderNo} has been {statusMsg}.");
                }
            }
            return result;
        }
    }
}
