namespace PrivateECommerce.API.DTOs.Sales
{
    public class SalesOrderListDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }

        public string Status { get; set; } = null!;

        public decimal TotalAmount { get; set; }

        public CustomerDto Customer { get; set; } = null!;

        public DateTime? SalesApprovedAt { get; set; }
        public DateTime? AdminApprovedAt { get; set; }

        public string? RejectedReason { get; set; }

        public List<SalesOrderItemDto> Items { get; set; } = new();
    }
}
