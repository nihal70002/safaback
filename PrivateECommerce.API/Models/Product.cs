public class Product
{
    public int Id { get; set; }

    public string Name { get; set; }
    public string Category { get; set; }

    public string Description { get; set; } // key features (html or text)
    public string ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ProductVariant> Variants { get; set; }
}
