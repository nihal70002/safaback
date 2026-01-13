namespace PrivateECommerce.API.DTOs
{
    public class AdminUpdateProductDto
    {
        public required string Name { get; set; }

        public int CategoryId { get; set; }   // ✅ FIX

        public required string Description { get; set; }

        public required string ImageUrl { get; set; }
    }
}