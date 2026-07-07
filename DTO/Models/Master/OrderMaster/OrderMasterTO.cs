using DTO.Models.Master.AddressMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Models.Master.OrderMaster
{
    public class OrderMasterTO
    {
        public int IdOrderMaster { get; set; }
        public string OrderNo { get; set; }
        public int CustomerId { get; set; }
        public int IdStatus { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DeliveryCharge { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public int IdAddressMapping { get; set; }
        public string Remarks { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? UpdatedBy { get; set; }
        public List<OrderDetailsTO> OrderItems { get; set; }
        public TblPaymentTO PaymentDetail { get; set; } 
    }
    public class OrderListResponseTO
    {
        public int IdOrderMaster { get; set; }
        public string OrderNo { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int IdStatus { get; set; }
        public string StatusName { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DeliveryCharge { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public int IdAddressMapping { get; set; }
        public int IdOrderType { get; set; }
        public string Remarks { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? UpdatedBy { get; set; }
        public List<OrderItemResponseTO> orderItemResponseTO { get; set; }
        public TblPaymentResponseTO  TblPaymentResponseTO { get; set; } 
        public DeliveryAssignmentResponseTO DeliveryAssignment { get; set; } 
        public tblAddressResponseTO tblAddressResponseTO { get; set; }
    }
}
