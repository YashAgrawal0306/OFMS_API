using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Models.Master.OrderMaster
{
    public class UpdateOrderStatusRequest
    {
        public int IdOrderMaster { get; set; }
        public int IdStatus { get; set; }
        public int UpdatedBy { get; set; }
        public string? Remarks { get; set; }
    }
}
