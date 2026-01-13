public class CreateProductDto
{
    public string Name { get; set; }
    public int CategoryId { get; set; }   // ✅ int
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}
