using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Models.Master.ItemMaster
{
    public class TblCategoryMasterTO
    {
        public int IdCategory { get; set; }
        public int IdGroupMaster { get; set; }
        public int? ParentId { get; set; }
        public string? CategoryName { get; set; }
        public string? CatDescription { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
