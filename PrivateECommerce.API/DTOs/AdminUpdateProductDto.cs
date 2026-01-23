namespace PrivateECommerce.API.DTOs
{
    public class AdminUpdateProductDto
    {
        public required string Name { get; set; }

        public int CategoryId { get; set; }

        public int BrandId { get; set; }   // ✅ ADD THIS

        public required string Description { get; set; }

        public required string ImageUrl { get; set; }
    }
}
