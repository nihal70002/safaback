using System.ComponentModel.DataAnnotations.Schema;

namespace PrivateECommerce.API.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        // Navigation property: marked as null! because EF Core will 
        // load this from the database.
        public User User { get; set; } = null!;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Pending";

        // Initialize the collection to avoid null reference warnings
        public ICollection<OrderItem> OrderItems { get; set; } = [];
    }
}