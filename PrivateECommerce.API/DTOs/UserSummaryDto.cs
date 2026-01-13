namespace PrivateECommerce.API.DTOs
{
    // High-level info for the customer list sidebar
    public class UserSummaryDto
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }

    // Detailed info including full order history
    public class UserDetailsDto : UserSummaryDto
    {
        public DateTime JoinDate { get; set; }
        public List<OrderHistoryDto> OrderHistory { get; set; } = new();
    }

    public class OrderHistoryDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
    }
}