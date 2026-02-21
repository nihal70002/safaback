using System.ComponentModel.DataAnnotations;

namespace ClientEcommerce.API.DTOs
{
    public class CreateCategoryDto
    {
        [Required]
        public required string Name { get; set; }
    }
}