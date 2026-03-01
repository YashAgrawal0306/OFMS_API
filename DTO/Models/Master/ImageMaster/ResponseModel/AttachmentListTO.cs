using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Models.Master.ImageMaster.ResponseModel
{
    public class AttachmentListTO
    {
        public int IdItemMasterImage { get; set; }
        public int ImageTypeId { get; set; }
        public string? ImageUrl { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsMain { get; set; } = false;
        public int ReferenceId { get; set; }

    }
}
