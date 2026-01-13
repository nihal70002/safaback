using System.ComponentModel.DataAnnotations;

namespace PrivateECommerce.API.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public required string Name { get; set; }

        public bool IsActive { get; set; } = true;
    }
}