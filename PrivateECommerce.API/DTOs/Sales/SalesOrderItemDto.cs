namespace PrivateECommerce.API.DTOs.Sales
{
    public class SalesOrderItemDto
    {
        public string ProductName { get; set; } = null!;
        public string Size { get; set; } = null!;
        public int Quantity { get; set; }

        // ✅ ADD THIS
        public string ProductCode { get; set; } = null!;
    }

}
