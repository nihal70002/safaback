namespace PrivateECommerce.API.DTOs
{
    public class ProductDetailDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }

        public int CategoryId { get; set; }        // ✅ FIX
        public string CategoryName { get; set; }   // ✅ FIX

        public string Description { get; set; }
        public string ImageUrl { get; set; }

        public List<ProductVariantDto> Sizes { get; set; }
    }
}
