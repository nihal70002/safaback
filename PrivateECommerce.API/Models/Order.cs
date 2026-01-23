using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PrivateECommerce.API.Models
{
    public class Order
    {
        public int Id { get; set; }

        // =======================
        // CUSTOMER
        // =======================
        public int UserId { get; set; }

        [JsonIgnore]
        public User User { get; set; } = null!;

        // =======================
        // SALES EXECUTIVE
        // =======================
        public bool? IsRejectedBySales { get; set; }

        public int? SalesExecutiveId { get; set; }

        [JsonIgnore]
        public User? SalesExecutive { get; set; }
        public string? RejectedReason { get; set; }

        // null   → Pending by Sales
        // not null → Approved by Sales
        public DateTime? SalesApprovedAt { get; set; }

        // =======================
        // ADMIN WORKFLOW
        // =======================
        public string Status { get; set; } = "PendingSalesApproval";
        // Confirmed | Dispatched | Delivered

        public DateTime? AdminApprovedAt { get; set; }

        // ✅ ADD THIS
        public DateTime? DeliveredAt { get; set; }

        // =======================
        // ORDER INFO
        // =======================

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
