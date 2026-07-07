using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Models.Master.OrderMaster
{
    public class OrderDetailsTO
    {
        public int IdOrderDetails { get; set; }
        public int IdOrderMaster { get; set; }
        public int IdItemMaster { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedOn { get; set; }
    }
    public class OrderItemResponseTO
    {
        public int IdOrderDetails { get; set; }
        public int IdItemMaster { get; set; }
        public string ItemName { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        public CookAssignmentTO CookAssignment { get; set; }
    }
}
