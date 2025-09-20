namespace OFMS_API.Models
{
    public class FilterModelTO
    {
        public int? PageNo { get; set; }
        public int? PageSize { get; set; }
        public string? SearchText { get; set; }
        public string? SortColumn { get; set; }
        public string? SortOrder { get; set; }
        public string? Flag { get; set; }
        public bool? isActive { get; set; } = true;
        public int? CategoryId { get; set; }

    }
    public class OutPutClass<T> where T : class
    {
        public List<T>? List { get; set; }
        public int TotalUser { get; set; }
    }
}
