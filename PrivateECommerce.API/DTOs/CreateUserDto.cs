using System.ComponentModel.DataAnnotations;

namespace PrivateECommerce.API.DTOs
{
    public class CreateUserDto
    {
        [Required]
        [StringLength(150)]
        public required string Name { get; set; }

        [StringLength(150)]
        public string? CompanyName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public required string Email { get; set; }

        [Required]
        [Phone]
        [StringLength(30)]
        public required string PhoneNumber { get; set; }

        [Required]
        [StringLength(128, MinimumLength = 8)]
        public required string Password { get; set; }
    }
}
