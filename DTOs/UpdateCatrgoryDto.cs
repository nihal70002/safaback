using System.ComponentModel.DataAnnotations;

namespace ClientEcommerce.API.DTOs
{
    public class UpdateCategoryDto
    {
        [Required]
        public required string Name { get; set; }

        public bool IsActive { get; set; }
    }
}