using DTO.Models.Master.ImageMaster.ResponseModel;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Models.Master.ItemMaster.ResponseModel
{
    public class TblGroupMasterResponseTO
    {
        public int IdGroupMaster { get; set; }
        public string? GroupName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? UpdatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public string? UpdatedByName { get; set; }
        public List<AttachmentListTO>? AttachmentTo { get; set; }
    }
}
