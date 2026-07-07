using DTO.Models.CommonModel;
using DTO.Models.Master.OrderMaster;
using Repository.DAL.Interface.Master.DeliveryAssignment;
using Services.BL.Interface.Master.DeliveryAssignment;
using System;
using System.Threading.Tasks;

namespace Services.BL.Imple.Master.DeliveryAssignment
{
    public class DeliveryAssignmentBL : IDeliveryAssignmentBL
    {
        private readonly IDeliveryAssignmentDAL _dal;

        public DeliveryAssignmentBL(IDeliveryAssignmentDAL dal)
        {
            _dal = dal;
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

            return await _dal.AssignDeliveryBoy(model);
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
            return await _dal.AcceptDelivery(idDeliveryAssignment, updatedBy);
        }

        public async Task<int> PickUpOrder(int idDeliveryAssignment, int updatedBy)
        {
            await ValidateNotDelivered(idDeliveryAssignment);
            return await _dal.PickUpOrder(idDeliveryAssignment, updatedBy);
        }

        public async Task<int> MarkDelivered(int idDeliveryAssignment, int updatedBy)
        {
            await ValidateNotDelivered(idDeliveryAssignment);
            return await _dal.MarkDelivered(idDeliveryAssignment, updatedBy);
        }

        private async Task ValidateNotDelivered(int idDeliveryAssignment)
        {
            bool isDelivered = await _dal.CheckIfDelivered(idDeliveryAssignment);
            if (isDelivered)
            {
                throw new Exception("Delivered order cannot be updated.");
            }
        }
    }
}
