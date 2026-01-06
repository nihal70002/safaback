using System.ComponentModel.DataAnnotations;

namespace PrivateECommerce.API.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        public string CompanyName { get; set; }


        [Required, EmailAddress]
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string Role { get; set; } // Admin / Customer

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Order> Orders { get; set; }
    }
}
