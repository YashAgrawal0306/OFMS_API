namespace OFMS_API.Models.DTO
{
    public class MyCartDTO
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public int MenuItemId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string? MenuItemName { get; set; }
        public string? MenuItemDescription { get; set; }
        public string? MenuItemImageUrl { get; set; }
        public decimal? MenuItemPrice { get; set; }

    }
}
