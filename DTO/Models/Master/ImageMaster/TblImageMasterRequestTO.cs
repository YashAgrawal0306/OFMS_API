using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Models.Master.ImageMaster
{
    public class TblImageMasterRequestTO
    {
        public int IdItemMasterImage { get; set; }
        public int ImageTypeId { get; set; }
        public int ReferenceId { get; set; }
        public IFormFile? ImageUrl { get; set; }
        public bool IsMain { get; set; } 
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; } 
    }
}
