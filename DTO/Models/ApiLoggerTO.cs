namespace OFMS_API.Models
{
    public class ApiLoggerTO
    {
        public int Id { get; set; }
        public DateTime? TimeStamp { get; set; }
        public int? ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string APIName { get; set; }
        public int? CreatedBy { get; set; }
        public string RequestPayload { get; set; }
        public string Response { get; set; }
        public DateTime? RequestTime { get; set; }
        public DateTime? ResponseTime { get; set; }
        public string ErrorCode { get; set; }
        public decimal? ElapsedTime { get; set; }
        public string ExceptionMessage { get; set; }
    }
}
