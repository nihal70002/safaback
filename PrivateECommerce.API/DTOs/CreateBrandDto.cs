using System.ComponentModel.DataAnnotations;

namespace PrivateECommerce.API.DTOs
{
    public class CreateBrandDto
    {
        [Required]
        [StringLength(150)]
        public string BrandName { get; set; } = null!;
    }
}
