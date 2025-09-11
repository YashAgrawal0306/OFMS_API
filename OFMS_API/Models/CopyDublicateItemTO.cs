namespace OFMS_API.Models
{
    public class CopyDublicateItemTO
    {
        public int MenuId { get; set; }
        public string? MenuNewName { get; set; }
        public int CopyPricingInfo { get; set; }
        public int Copyingredients { get; set; }
        public bool ActiveStatus {  get; set; }
    }
}
