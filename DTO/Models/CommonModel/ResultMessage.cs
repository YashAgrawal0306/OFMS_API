using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Models.CommonModel
{
    public class ResultMessage
    {
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = "Operation completed successfully.";
        public object? Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public string? ErrorCode { get; set; }
        public int StatusCode { get; set; } = 200;
    }
}
