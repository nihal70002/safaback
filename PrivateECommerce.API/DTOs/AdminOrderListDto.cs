namespace PrivateECommerce.API.DTOs
{
    public class AdminOrderListDto
    {
        public int OrderId { get; set; }

        public required string CustomerName { get; set; }
        public required string CompanyName { get; set; }

        public DateTime OrderDate { get; set; }

        // This is already correct as nullable (?)
        public string? PhoneNumber { get; set; }

        public required string Status { get; set; }
        public decimal TotalAmount { get; set; }
    }
}