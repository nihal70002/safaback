namespace PrivateECommerce.API.DTOs.Sales
{
    public class CustomerOrderItemDto
    {
        public string ProductName { get; set; } = null!;
        public string Size { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        // ✅ REQUIRED
        public string ProductCode { get; set; } = null!;
    }

}
