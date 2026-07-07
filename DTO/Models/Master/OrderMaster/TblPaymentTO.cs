using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Models.Master.OrderMaster
{
    public class TblPaymentTO
    {
        public int IdPayment { get; set; }
        public int IdOrderMaster { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TransactionNo { get; set; }
        public string? TransactionTypeId { get; set; }

        public int IdStatus { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class TblPaymentResponseTO
    {
        public int IdPayment { get; set; }
        public int IdOrderMaster { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TransactionNo { get; set; }
        public string? TransactionTypeId { get; set; }

        public int IdStatus { get; set; }
        public string? StatusName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
