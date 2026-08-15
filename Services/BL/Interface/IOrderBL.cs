using DTO.Models.CommonModel;
using DTO.Models.Master.OrderMaster;
using OFMS_API.Models;
using OFMS_API.Models.DTO;

namespace OFMS_API.BL.Interface
{
    public interface IOrderBL
    {
        //New Order Master
        Task<ResultMessage> AddOrderMaster(OrderMasterTO orderMasterTO);
        Task<OutPutClass<OrderListResponseTO>> GetOrderMasterList(OrderListFilter orderListFilter);
        Task<OrderListResponseTO> GetOrderMasterListByIdOrder(int IdOrderMaster);
        Task<bool> UpdateOrderMaster(OrderMasterTO order);
        Task<ResultMessage> UpdateOrderStatus(UpdateOrderStatusRequest request);
        Task<byte[]> GenerateOrderInvoiceAsync(int IdOrderMaster);
    }
}
