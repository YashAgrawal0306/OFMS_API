using DTO.Models.CommonModel;
using DTO.Models.Master.OrderMaster;
using System.Threading.Tasks;

namespace Services.BL.Interface.Master.DeliveryAssignment
{
    public interface IDeliveryAssignmentBL
    {
        Task<int> AssignDeliveryBoy(CreateDeliveryAssignmentTO model);
        Task<DeliveryAssignmentResponseTO> GetDeliveryAssignmentById(int idDeliveryAssignment);
        Task<OutPutClass<DeliveryAssignmentResponseTO>> GetAllDeliveryAssignments(FilterModelTO filter);
        Task<int> AcceptDelivery(int idDeliveryAssignment, int updatedBy);
        Task<int> PickUpOrder(int idDeliveryAssignment, int updatedBy);
        Task<int> MarkDelivered(int idDeliveryAssignment, int updatedBy);
    }
}
