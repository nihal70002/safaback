namespace PrivateECommerce.API.DTOs
{
    public class CreateProductVariantDto
    {
        public required string Size { get; set; }
        public int Stock { get; set; }
        public decimal Price { get; set; }
    }
}