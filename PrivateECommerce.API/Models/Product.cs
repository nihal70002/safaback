using System.ComponentModel.DataAnnotations.Schema;

namespace PrivateECommerce.API.Models
{
    [Table("Products")]
    public class Product
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? NameArabic { get; set; }
        public string? ProductCode { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public required string Description { get; set; }
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();


        public bool IsActive { get; set; } = true;
        public int BrandId { get; set; }
        public Brand Brand { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    }
}
