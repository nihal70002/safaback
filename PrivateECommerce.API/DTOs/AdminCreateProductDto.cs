using System.ComponentModel.DataAnnotations;

namespace PrivateECommerce.API.DTOs
{
    public class AdminCreateProductDto
    {
        [Required]
        [StringLength(200)]
        public required string Name { get; set; }

        [StringLength(200)]
        public string? NameArabic { get; set; }

        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }

        [Range(1, int.MaxValue)]
        public int BrandId { get; set; }

        [Required]
        [StringLength(2000)]
        public required string Description { get; set; }

        [MaxLength(5)]
        public List<string> ImageUrls { get; set; } = new();

        [Required]
        [MinLength(1)]
        public List<AdminCreateProductVariantDto> Variants { get; set; } = new();
    }
}
