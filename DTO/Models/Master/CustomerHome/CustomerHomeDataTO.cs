namespace DTO.Models.Master.CustomerHome
{
    public class CustomerHomeDataTO
    {
        public MostOrderedDishTO? MostOrderedDish { get; set; }
        public FamousCookTO? FamousCook { get; set; }
        public AdminContactTO? AdminContact { get; set; }
        public PromoItemTO? PromoItem1 { get; set; }
        public PromoItemTO? PromoItem2 { get; set; }
    }

    public class MostOrderedDishTO
    {
        public int IdItemMaster { get; set; }
        public string? ItemName { get; set; }
        public string? ItemDescription { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? CategoryName { get; set; }
        public int TotalQtyOrdered { get; set; }
    }

    public class FamousCookTO
    {
        public int CookUserId { get; set; }
        public string? CookName { get; set; }
        public string? CookEmail { get; set; }
        public string? CookPhone { get; set; }
        public string? ProfileImage { get; set; }
        public int TotalAssignedOrders { get; set; }
    }

    public class AdminContactTO
    {
        public string? AdminName { get; set; }
        public string? AdminEmail { get; set; }
        public string? AdminPhone { get; set; }
        public string? ProfileImage { get; set; }
    }

    public class PromoItemTO
    {
        public int IdItemMaster { get; set; }
        public string? ItemName { get; set; }
        public string? ItemDescription { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
    }
}
