using DTO.Models.CommonModel;
using DTO.Models.Master.OrderMaster;
using System.Threading.Tasks;

namespace Repository.DAL.Interface.Master.DeliveryAssignment
{
    public interface IDeliveryAssignmentDAL
    {
        Task<int> AssignDeliveryBoy(CreateDeliveryAssignmentTO model);
        Task<DeliveryAssignmentResponseTO> GetDeliveryAssignmentById(int idDeliveryAssignment);
        Task<OutPutClass<DeliveryAssignmentResponseTO>> GetAllDeliveryAssignments(FilterModelTO filter);
        Task<int> AcceptDelivery(int idDeliveryAssignment, int updatedBy);
        Task<int> PickUpOrder(int idDeliveryAssignment, int updatedBy);
        Task<int> MarkDelivered(int idDeliveryAssignment, int updatedBy);
        Task<bool> CheckOrderExists(int idOrderMaster);
        Task<bool> CheckDeliveryBoyExists(int deliveryBoyUserId);
        Task<bool> CheckDuplicateAssignment(int idOrderMaster);
        Task<bool> CheckIfDelivered(int idDeliveryAssignment);
        Task<bool> AcceptDelivery(ActionDeliveryTO payload);
        Task<bool> PickUpOrder(ActionDeliveryTO payload);
        Task<bool> MarkDelivered(ActionDeliveryTO payload);
        Task<DeliveryDashboardCountsTO> GetDashboardCounts(int deliveryUserId);
    }
}
