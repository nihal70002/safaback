namespace PrivateECommerce.API.DTOs.Sales
{
    public class SalesOrderItemDto
    {
        public string ProductName { get; set; } = null!;
        public string Size { get; set; } = null!;
        public int Quantity { get; set; }


        public string? Class { get; set; }
        public string? Style { get; set; }
        public string? Material { get; set; }
        public string? Color { get; set; }


        public decimal UnitPrice { get; set; }
        // ✅ ADD THIS
        public string ProductCode { get; set; } = null!;
    }

}
