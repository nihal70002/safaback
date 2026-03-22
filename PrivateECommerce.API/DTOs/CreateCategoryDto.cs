using System.ComponentModel.DataAnnotations;

namespace PrivateECommerce.API.DTOs
{
    public class CreateCategoryDto
    {
        [Required]
        public required string Name { get; set; }
    }
}