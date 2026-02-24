using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Models.Master.ItemMaster
{
    public class TblItemMasterTO
    {
        public int IdItemMaster { get; set; }
        public int IdCategory { get; set; }
        public int IdSubCategory { get; set; }
        public int IdGroupMaster { get; set; }
        public string? ItemName { get; set; }
        public string? ItemDescription { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
