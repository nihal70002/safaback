namespace PrivateECommerce.API.DTOs
{
    public class ProductVariantListDto
    {
        public int VariantId { get; set; }
        public string Size { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
