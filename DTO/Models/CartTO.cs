using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OFMS_API.Models
{
    public class CartTO
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public int MenuItemId { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal Price { get; set; } 
        public bool Status { get; set; } = true;  
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}
