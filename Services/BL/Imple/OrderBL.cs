using DTO.Models.CommonModel;
using DTO.Models.Master.OrderMaster;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OFMS_API.BL.Interface;
using OFMS_API.DAL.Imple;
using OFMS_API.DAL.Interface;
using OFMS_API.Models;
using OFMS_API.Models.DTO;

namespace OFMS_API.BL.Imple
{
    public class OrderBL : IOrderBL
    {
        private readonly string connq;
        private readonly IOrderDAL _iOrderDAL;
        public OrderBL(IOrderDAL iOrderDAL,IConfiguration configuration)
        {
            _iOrderDAL = iOrderDAL;
            connq = configuration.GetConnectionString("DefaultConnection") ?? "";
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
                    tran.Commit();
                    resultMessage.IsSuccess = true;
                    resultMessage.Message = "Order Added Successfully";
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
    }
}
