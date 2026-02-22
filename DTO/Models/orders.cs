namespace OFMS_API.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int? DeliveryStaffId { get; set; } 
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "pending";  
        public string PaymentStatus { get; set; } = "unpaid"; 
        public DateTime OrderTime { get; set; } = DateTime.Now;
    }
}
