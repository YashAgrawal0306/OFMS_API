using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Models.Master.OrderMaster
{
    public class TblDeliveryAssignmentTO
    {
        public int IdDeliveryAssignment { get; set; }

        public int IdOrderMaster { get; set; }

        public int DeliveryBoyUserId { get; set; }

        public int IdStatus { get; set; }

        public DateTime AssignedOn { get; set; }

        public DateTime? AcceptedOn { get; set; }

        public DateTime? PickedUpOn { get; set; }

        public DateTime? DeliveredOn { get; set; }

        public int? EstimatedDeliveryTime { get; set; }

        public int? ActualDeliveryTime { get; set; }

        public string? DeliveryRemarks { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedOn { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public int? UpdatedBy { get; set; }
    }
    public class CreateDeliveryAssignmentTO
    {
        public int IdOrderMaster { get; set; }

        public int DeliveryBoyUserId { get; set; }

        public int IdStatus { get; set; }

        public int? EstimatedDeliveryTime { get; set; }

        public string? DeliveryRemarks { get; set; }

        public int? CreatedBy { get; set; }
    }


    public class UpdateDeliveryStatusTO
    {
        public int IdDeliveryAssignment { get; set; }

        public int IdStatus { get; set; }

        public string? DeliveryRemarks { get; set; }

        public int? UpdatedBy { get; set; }
    }
    public class DeliveryAssignmentResponseTO
    {
        public int IdDeliveryAssignment { get; set; }

        public int IdOrderMaster { get; set; }

        public string? OrderNo { get; set; }

        public int DeliveryBoyUserId { get; set; }

        public string? DeliveryBoyName { get; set; }

        public int IdStatus { get; set; }

        public string? StatusName { get; set; }

        public DateTime AssignedOn { get; set; }

        public DateTime? AcceptedOn { get; set; }

        public DateTime? PickedUpOn { get; set; }

        public DateTime? DeliveredOn { get; set; }

        public int? EstimatedDeliveryTime { get; set; }

        public int? ActualDeliveryTime { get; set; }

        public string? DeliveryRemarks { get; set; }
    }

    public class ActionDeliveryTO
    {
        public int IdDeliveryAssignment { get; set; }
        public int UpdatedBy { get; set; }
    }

    public class DeliveryDashboardCountsTO
    {
        public int AssignedOrders { get; set; }
        public int PickedUpOrders { get; set; }
        public int DeliveredToday { get; set; }
        public int PendingOrders { get; set; }

        // Expanded properties for delivery dashboard metrics
        public int TotalAssigned { get; set; }
        public int PendingDeliveries { get; set; }
        public int AcceptedDeliveries { get; set; }
        public int PickedUpDeliveries { get; set; }
        public int OutForDelivery { get; set; }
        public int FailedDeliveries { get; set; }
    }
}
