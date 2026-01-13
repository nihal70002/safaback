using System.ComponentModel.DataAnnotations;

namespace PrivateECommerce.API.DTOs
{
    public class CreateCategoryDto
    {
        [Required]
        public string Name { get; set; }
    }
}
