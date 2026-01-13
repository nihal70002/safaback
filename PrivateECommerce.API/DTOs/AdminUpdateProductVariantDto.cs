namespace PrivateECommerce.API.DTOs
{
    public class AdminUpdateProductVariantDto
    {
        public required string Size { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}