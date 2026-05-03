using System.ComponentModel.DataAnnotations;

namespace PrivateECommerce.API.DTOs.Auth
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = null!;
    }
}
