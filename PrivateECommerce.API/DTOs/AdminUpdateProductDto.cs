namespace PrivateECommerce.API.DTOs
{
    public class AdminUpdateProductDto
    {
        public string Name { get; set; }
        public int CategoryId { get; set; }   // ✅ FIX
        public string Description { get; set; }
        public string ImageUrl { get; set; }

    }
}
