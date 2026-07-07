using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Models.Master.AddressMaster
{
    public class tblAddressMappingTO
    {
        public int IdAddressMapping { get; set; }

        public string EntityType { get; set; }
        public int EntityId { get; set; }

        public int IdAddress { get; set; }
        public int IdAddressType { get; set; }

        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
