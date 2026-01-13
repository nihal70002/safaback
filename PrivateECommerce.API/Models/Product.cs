using PrivateECommerce.API.Models;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; }

    // ✅ Category relation
    public int CategoryId { get; set; }
    public Category Category { get; set; }

    public string Description { get; set; }
    public string ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ProductVariant> Variants { get; set; }
}
