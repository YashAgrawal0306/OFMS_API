using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Models.Master.UserMaster
{
    public class TblUserRoleMappingTO
    {
        public int IdUserRoleMapping { get; set; }

        public int UserId { get; set; }

        public int RoleId { get; set; }

        public DateTime CreatedOn { get; set; }

        public bool IsActive { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public int CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }
    }
}
