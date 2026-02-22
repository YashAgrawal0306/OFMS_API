using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Models.CommonModel
{
    public class PagedResult<T>
    {
        public List<T>? Data { get; set; }
        public int TotalCount { get; set; }
        public int PageNo { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
