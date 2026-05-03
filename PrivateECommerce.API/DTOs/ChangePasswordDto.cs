using System.ComponentModel.DataAnnotations;

namespace PrivateECommerce.API.DTOs
{
    public class ChangePasswordDto
    {
        [Required]
        [StringLength(128, MinimumLength = 6)]
        public required string CurrentPassword { get; set; }

        [Required]
        [StringLength(128, MinimumLength = 8)]
        public required string NewPassword { get; set; }
    }
}
