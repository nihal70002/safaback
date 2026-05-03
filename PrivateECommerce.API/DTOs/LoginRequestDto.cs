using System.ComponentModel.DataAnnotations;

namespace PrivateECommerce.API.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        [StringLength(256)]
        public required string LoginId { get; set; }

        [Required]
        [StringLength(128, MinimumLength = 6)]
        public required string Password { get; set; }
    }
}
