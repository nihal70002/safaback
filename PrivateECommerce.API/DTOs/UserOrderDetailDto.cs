namespace PrivateECommerce.API.DTOs
{
    public class UserOrderDetailDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }

        public List<UserOrderItemDto> Items { get; set; }
    }

    public class UserOrderItemDto
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
