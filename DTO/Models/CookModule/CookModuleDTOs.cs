using DTO.Models.CommonModel;
using System;
using System.Collections.Generic;

namespace DTO.Models.CookModule
{
    public class CookDashboardCountsTO
    {
        public int AssignedOrders { get; set; }
        public int PreparingOrders { get; set; }
        public int ReadyOrders { get; set; }
        public int CompletedToday { get; set; }
        public int PendingOrders { get; set; }
    }

    public class CookOrderListResponseTO
    {
        public int IdCookAssignment { get; set; }
        public int IdOrderMaster { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public DateTime? AssignedOn { get; set; }
        public DateTime? AcceptedOn { get; set; }
        public int? EstimatedPreparationTime { get; set; }
        public int IdStatus { get; set; }
        public string StatusName { get; set; }
        public string StatusColorCode { get; set; }
        public int TotalItems { get; set; }
        public DateTime? CompletedOn { get; set; } // Map to ReadyOn or actual completion
        public bool IsMerged { get; set; }
    }

    public class CookOrderDetailResponseTO
    {
        public int IdCookAssignment { get; set; }
        public int IdOrderMaster { get; set; }
        public string OrderNumber { get; set; }
        public DateTime? OrderDate { get; set; }
        
        // Customer Info
        public string CustomerName { get; set; }
        public string ContactNumber { get; set; }
        public string DeliveryAddress { get; set; }

        // Assignment Info
        public DateTime? AssignedOn { get; set; }
        public DateTime? AcceptedOn { get; set; }
        public DateTime? StartCookingOn { get; set; }
        public DateTime? ReadyOn { get; set; }
        public int? EstimatedPreparationTime { get; set; }
        public int? ActualPreparationTime { get; set; }
        public string CookRemark { get; set; }

        // Status Info
        public int IdStatus { get; set; }
        public string StatusName { get; set; }
        public string StatusColorCode { get; set; }
        public int OrderStatusId { get; set; }
        
        // Items
        public List<CookOrderItemTO> Items { get; set; } = new List<CookOrderItemTO>();
    }

    public class CookOrderItemTO
    {
        public int IdOrderDetails { get; set; }
        public string ItemName { get; set; }
        public int Qty { get; set; }
        public string ItemRemark { get; set; }
        public int IdCookAssignment { get; set; }
        public int IdStatus { get; set; }
        public string StatusName { get; set; }
    }

    public class AcceptOrderRequestTO
    {
        public int IdOrderMaster { get; set; }
        public List<int>? IdCookAssignments { get; set; }
        public int EstimatedMinutes { get; set; }
        public string Remark { get; set; }
    }

    public class UpdateCookingStatusRequestTO
    {
        public int IdOrderMaster { get; set; }
        public List<int>? IdCookAssignments { get; set; }
        public int NewStatusId { get; set; }
        public string Remark { get; set; }
    }

    public class UpdateEstimatedTimeRequestTO
    {
        public int IdOrderMaster { get; set; }
        public int EstimatedMinutes { get; set; }
    }

    public class CookReportFilterTO
    {
        public int? CookUserId { get; set; }
        public string? FilterType { get; set; } // "All", "Day", "Month", "Year", "Range"
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? SelectedMonth { get; set; } // 1-12
        public int? SelectedYear { get; set; }  // e.g. 2026
        public string? SearchText { get; set; }
        public int? IdStatus { get; set; }
    }

    public class CookReportItemTO
    {
        public int IdCookAssignment { get; set; }
        public int IdOrderMaster { get; set; }
        public string OrderNo { get; set; }
        public string CustomerName { get; set; }
        public int CookUserId { get; set; }
        public string CookName { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public DateTime AssignedOn { get; set; }
        public DateTime? AcceptedOn { get; set; }
        public DateTime? StartCookingOn { get; set; }
        public DateTime? ReadyOn { get; set; }
        public int? EstimatedPreparationTime { get; set; }
        public int? ActualPreparationTime { get; set; }
        public int IdStatus { get; set; }
        public string StatusName { get; set; }
        public bool IsMerged { get; set; }
        public string Remarks { get; set; }
    }
}
