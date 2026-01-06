using System.ComponentModel.DataAnnotations.Schema;

namespace PrivateECommerce.API.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }   // 🔥 REQUIRED

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Pending";

        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
