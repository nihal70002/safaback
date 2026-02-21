namespace ClientEcommerce.API.DTOs
{
    public class ProductVariantListDto
    {
        public int VariantId { get; set; }
        public required string Size { get; set; }

        public string? ProductCode { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}