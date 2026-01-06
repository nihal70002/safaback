public class ProductDetailDto
{
    public int ProductId { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }

    public List<ProductVariantDto> Sizes { get; set; }
}
