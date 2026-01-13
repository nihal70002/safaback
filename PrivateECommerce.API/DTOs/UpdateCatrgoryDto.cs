using System.ComponentModel.DataAnnotations;

namespace PrivateECommerce.API.DTOs
{
    public class UpdateCategoryDto
    {
        [Required]
        public string Name { get; set; }

        public bool IsActive { get; set; }
    }
}
