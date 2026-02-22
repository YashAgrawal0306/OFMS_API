using Microsoft.AspNetCore.Http;

namespace OFMS_API.Models
{
    public class GlobalResponseModel<T>
    {
        public string message { get; set; } = "Request completed successfully.";
        public int statusCode { get; set; } = StatusCodes.Status200OK;
        public string status { get; set; } = "Success";
        public T data { get; set; } = default!;
        public List<string> errors { get; set; } = [];
        public Exception? exception { get; set; }

        public static readonly object[] blankArray = Array.Empty<object>();
    }
}
