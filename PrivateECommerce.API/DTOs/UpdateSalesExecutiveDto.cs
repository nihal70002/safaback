using System.ComponentModel.DataAnnotations;

namespace PrivateECommerce.API.DTOs.Admin
{
    public class UpdateSalesExecutiveDto
    {
        [Required]
        public string Name { get; set; } = null!;

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string PhoneNumber { get; set; } = null!;

        [Required]
        public string CompanyName { get; set; } = null!;

        public bool IsActive { get; set; }

        // OPTIONAL – admin can change password
        public string? NewPassword { get; set; }
    }
}
