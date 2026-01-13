using PrivateECommerce.API.Models;

namespace PrivateECommerce.API.Models
{
    public class Product
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        // ✅ Category relation
        public int CategoryId { get; set; }

        // Navigation properties are best initialized as null! to satisfy the compiler
        // while EF handles the actual loading.
        public Category Category { get; set; } = null!;

        public required string Description { get; set; }
        public required string ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Initialize collections to prevent null reference errors
        public ICollection<ProductVariant> Variants { get; set; } = [];
    }
}