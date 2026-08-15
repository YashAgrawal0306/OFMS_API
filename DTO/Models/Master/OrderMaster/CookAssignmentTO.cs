using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Models.Master.OrderMaster
{
    public class CookAssignmentTO
    {
        public int IdCookAssignment { get; set; }

        public int? IdOrderMaster { get; set; }

        public int? IdOrderDetails { get; set; }
        // NULL = Whole Order Assignment OR Merged Assignment

        public int? IdItemMaster { get; set; }

        public int? TotalQuantity { get; set; }

        public bool IsMerged { get; set; }
        // Value = Specific Item Assignment

        public int CookUserId { get; set; }

        public int IdStatus { get; set; }

        public DateTime AssignedOn { get; set; }

        public DateTime? AcceptedOn { get; set; }

        public DateTime? StartCookingOn { get; set; }

        public DateTime? ReadyOn { get; set; }

        public int? EstimatedPreparationTime { get; set; }

        public int? ActualPreparationTime { get; set; }

        public string? Remarks { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedOn { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public int? UpdatedBy { get; set; }

        public string? CookName { get; set; }
    }
    public class CreateCookAssignmentTO
    {
        public int IdOrderMaster { get; set; }

        public int? IdOrderDetails { get; set; }

        public int CookUserId { get; set; }

        public int IdStatus { get; set; }

        public int? EstimatedPreparationTime { get; set; }

        public string? Remarks { get; set; }
    }

    public class UpdateKitchenStatusTO
    {
        public int IdCookAssignment { get; set; }

        public int IdStatus { get; set; }

        public string? Remarks { get; set; }

        public int? UpdatedBy { get; set; }
    }

    public class CookAssignmentResponseTO
    {
        public int IdCookAssignment { get; set; }

        public int? IdOrderMaster { get; set; }

        public int? IdOrderDetails { get; set; }

        public int? IdItemMaster { get; set; }
        
        public int? TotalQuantity { get; set; }
        
        public bool IsMerged { get; set; }
        
        public string? MappedOrderDetailsIds { get; set; }
        
        public string? MappedOrderMasterIds { get; set; }
        
        public string? MappedOrderNos { get; set; }

        public string? OrderNo { get; set; }

        public int CookUserId { get; set; }

        public string? CookName { get; set; }

        public int IdStatus { get; set; }

        public string? StatusName { get; set; }

        public DateTime AssignedOn { get; set; }

        public DateTime? AcceptedOn { get; set; }

        public DateTime? StartCookingOn { get; set; }

        public DateTime? ReadyOn { get; set; }

        public int? EstimatedPreparationTime { get; set; }

        public int? ActualPreparationTime { get; set; }

        public string? Remarks { get; set; }
    }

    public class MergedCookAssignmentRequestTO
    {
        public int IdItemMaster { get; set; }
        public int CookUserId { get; set; }
        public int IdStatus { get; set; }
        public int? EstimatedPreparationTime { get; set; }
        public string? Remarks { get; set; }
        
        public List<MergedOrderItemTO> SourceOrders { get; set; } = new List<MergedOrderItemTO>();
    }

    public class MergedOrderItemTO
    {
        public int IdOrderMaster { get; set; }
        public int IdOrderDetails { get; set; }
        public int Quantity { get; set; }
    }

    public class MergeableItemResponseTO
    {
        public int IdItemMaster { get; set; }
        public string? ItemName { get; set; }
        public int TotalQuantity { get; set; }
        public int OrderCount { get; set; }
        public List<MergeableSourceOrderTO> Orders { get; set; } = new List<MergeableSourceOrderTO>();
    }

    public class MergeableSourceOrderTO
    {
        public int IdOrderMaster { get; set; }
        public string? OrderNo { get; set; }
        public int IdOrderDetails { get; set; }
        public int Quantity { get; set; }
        public string? CustomerName { get; set; }
        public int CurrentStatusId { get; set; }
        public string? CurrentStatusName { get; set; }
    }
}
