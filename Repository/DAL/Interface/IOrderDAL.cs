using DTO.Models.CommonModel;
using DTO.Models.Master.AddressMaster;
using DTO.Models.Master.OrderMaster;
using Microsoft.Data.SqlClient;
using OFMS_API.Models;
using OFMS_API.Models.DTO;

namespace OFMS_API.DAL.Interface
{
    public interface IOrderDAL
    {
        Task<int> AddOrderMaster(OrderMasterTO orderMasterTO,SqlConnection conn,SqlTransaction tran);
        Task<int> AddPaymentData(TblPaymentTO tblPaymentTO,SqlConnection conn,SqlTransaction tran);
        Task<OutPutClass<OrderListResponseTO>> GetOrderMasterList(OrderListFilter orderListFilter);
        Task<tblAddressResponseTO> GetAddressByIdAddressMapping(int idAddressMapping);
        Task<OrderListResponseTO> GetOrderMasterListByIdOrder(int IdOrderMaster);
        Task<bool> UpdateOrderMaster(OrderMasterTO order);
        Task<ResultMessage> UpdateOrderStatus(UpdateOrderStatusRequest request);
        Task<bool> RecalculateOrderStatusDAL(int IdOrderMaster);
    }
}
