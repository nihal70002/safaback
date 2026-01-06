public class AdminCreateProductDto
{
    public string Name { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }

    public List<AdminCreateProductVariantDto> Variants { get; set; }
}
