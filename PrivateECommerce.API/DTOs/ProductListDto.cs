namespace PrivateECommerce.API.DTOs
{
    public class ProductListDto
    {
        public int ProductId { get; set; }

        public required string Name { get; set; }

        public int CategoryId { get; set; }
        public required string CategoryName { get; set; }

        public required string ImageUrl { get; set; }

        public bool IsActive { get; set; }

        // Initializing with [] (empty list) is the best fix for Collections
        public List<ProductVariantListDto> Variants { get; set; } = [];
    }
}