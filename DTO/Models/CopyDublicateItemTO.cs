namespace OFMS_API.Models
{
    public class CopyDublicateItemTO
    {
        public int menuItemId { get; set; }
        public string? ProductName { get; set; }
        public bool CopyPricingInfo { get; set; }
        public bool Copyingredients { get; set; }
        public bool ActiveStatus {  get; set; }
    }
}
