namespace PrivateECommerce.API.DTOs
{
    public class ProductVariantDto
    {
        public int VariantId { get; set; }

        public required string Size { get; set; }

        public decimal Price { get; set; }

        public int AvailableStock { get; set; }
    }
}