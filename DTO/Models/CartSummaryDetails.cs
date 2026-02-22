namespace OFMS_API.Models
{
    public class CartSummaryDetails
    {
        public int CartItemCount { get; set; }
        public int CouponId { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal DeliveryCharge { get; set; }
        public decimal Tax { get; set; }
        public decimal GrandTotal { get; set; }
    }
}
