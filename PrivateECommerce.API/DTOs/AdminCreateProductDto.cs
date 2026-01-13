namespace PrivateECommerce.API.DTOs
{
    public class AdminCreateProductDto
    {
        public string Name { get; set; }

        public int CategoryId { get; set; }   // ✅ MUST BE int

        public string Description { get; set; }
        public string ImageUrl { get; set; }

        public List<AdminCreateProductVariantDto> Variants { get; set; }
    }
}
