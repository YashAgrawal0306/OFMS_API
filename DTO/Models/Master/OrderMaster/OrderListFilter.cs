using DTO.Models.CommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Models.Master.OrderMaster
{
    public class OrderListFilter:FilterModelTO
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public new DateTime? FromDate { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public new DateTime? ToDate { get; set; }

        public string? OrderNo { get; set; }
        public string? OrderStatus { get; set; }
        public string? OrderId { get; set; }
        public string? OrderName { get; set; }
        public string? CustomerId { get; set; }
        public string? CustomerName { get; set; } = string.Empty;
        public int? PaymentStatusId { get; set; }
        public string? OrderDesc { get; set; }
        public string? Fromdate { get; set; }
        public string? Todate { get; set; }
        public bool SkipDateFilter { get; set; } = false;
    }
}
