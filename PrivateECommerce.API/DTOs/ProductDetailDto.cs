namespace PrivateECommerce.API.DTOs
{
    public class ProductDetailDto
    {
        public int ProductId { get; set; }

        public required string Name { get; set; }

        public int CategoryId { get; set; }        // ✅ FIX
        public required string CategoryName { get; set; }   // ✅ FIX

        public required string Description { get; set; }
        public required string ImageUrl { get; set; }

        // Initializing the list ensures scannability and prevents null reference errors
        public List<ProductVariantDto> Sizes { get; set; } = [];
    }
}