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
        // WAREHOUSE WORKFLOW
        // =======================

        public int? WarehouseUserId { get; set; }

        [JsonIgnore]
        public User? WarehouseUser { get; set; }

        public DateTime? WarehouseApprovedAt { get; set; }

        public string? WarehouseRemarks { get; set; }

        public DateTime CreatedAt { get; set; }


        // =======================
        // ADMIN WORKFLOW
        // =======================
        public string Status { get; set; } = "PendingSalesApproval";
        // Confirmed | Dispatched | Delivered

        // WHO approved (Sales / Warehouse / Admin)
        public string? ApprovedByRole { get; set; }


        public DateTime? AdminApprovedAt { get; set; }

        // ✅ ADD THIS
        public DateTime? DeliveredAt { get; set; }

        // =======================
        // ORDER INFO
        // =======================
        public DateTime? DispatchedAt { get; set; }
        

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
