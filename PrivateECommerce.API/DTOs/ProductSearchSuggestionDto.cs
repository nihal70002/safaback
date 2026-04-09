namespace PrivateECommerce.API.DTOs
{
    public class ProductSearchSuggestionDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;

        public string? NameArabic { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string? PrimaryImageUrl { get; set; }
        public decimal? StartingPrice { get; set; }
    }
}