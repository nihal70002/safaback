using System.ComponentModel.DataAnnotations;

namespace PrivateECommerce.API.DTOs.Auth
{
    public class ResetPasswordDto
    {
        [Required]
        [StringLength(512)]
        public string Token { get; set; } = null!;

        [Required]
        [StringLength(128, MinimumLength = 8)]
        public string NewPassword { get; set; } = null!;
    }
}
