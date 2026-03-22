using System.ComponentModel.DataAnnotations;

namespace PrivateECommerce.API.DTOs
{
    public class CreateCustomerBySalesDto
    {
        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string CompanyName { get; set; } = null!;

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string PhoneNumber { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}
