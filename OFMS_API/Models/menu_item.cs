using System.ComponentModel.DataAnnotations;
using System;

namespace OFMS_API.Models
{
    public class menu_item
    {
        public int MenuItemId { get; set; }
        public string? MenuName { get; set; }
        public string? ProductName { get; set; }
        public int CategoryId { get; set; }
        public bool Status { get; set; } = true;
        public decimal Price { get; set; }
        public decimal? DiscountPercent { get; set; }
        public decimal FinalPrice { get;  set; }
        public string? Ingredients { get; set; }
        public string? Description { get; set; }
        public int? CookingTimeMinutes { get; set; }
        public string? ImageUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CategoryName {  get; set; }

    }
}
