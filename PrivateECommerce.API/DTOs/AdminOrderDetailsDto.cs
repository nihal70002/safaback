namespace PrivateECommerce.API.DTOs
{
    public class AdminOrderDetailDto
    {
        public int OrderId { get; set; }

        public required string CustomerName { get; set; }
        public required string CompanyName { get; set; }
        public required string PhoneNumber { get; set; }

        public DateTime OrderDate { get; set; }

        public required string Status { get; set; }
        public decimal TotalAmount { get; set; }

        // Initializing the list to avoid CS8618
        public List<AdminOrderItemDto> Items { get; set; } = [];
    }

    public class AdminOrderItemDto
    {
        public required string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

}