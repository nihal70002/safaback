namespace PrivateECommerce.API.DTOs
{
    public class ProductListDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        // 🔥 ADD THIS
        public List<ProductVariantListDto> Variants { get; set; }
    }
}
