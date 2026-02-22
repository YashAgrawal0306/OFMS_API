using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Models.Master.ItemMaster
{
    public class TblItemPriceHistoryTO
    {
        public int IdPriceHistory { get; set; }
        public int IdItem { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public int? ChangedBy { get; set; }
        public string? ChangeReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
