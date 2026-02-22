namespace DTO.Models.CommonModel
{
    public class FilterModelTO
    {
        public int? PageNo { get; set; } = 1;
        public int? PageSize { get; set; } = 10;
        public string? SearchText { get; set; } = "";
        public string? SortColumn { get; set; }
        public string? SortOrder { get; set; }
        public string? Flag { get; set; }
        public bool? isActive { get; set; } = true;
        public int? CategoryId { get; set; } = 0;
        public int? RoleId { get; set; }

    }
    public class OutPutClass<T> where T : class
    {
        public List<T>? List { get; set; }
        public int? TotalUser { get; set; }
    }
}
