using System.Collections.Generic;
using PrivateECommerce.API.DTOs.Sales;

namespace PrivateECommerce.API.DTOs.Admin
{
    public class CustomerOrderDto
    {
        // ======================
        // CUSTOMER INFO
        // ======================
        public int CustomerId { get; set; }
        public string Name { get; set; } = null!;
        public string CompanyName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;


        // ======================
        // SUMMARY
        // ======================
        public int TotalOrders { get; set; }
        public decimal TotalAmount { get; set; }

        // ======================
        // ORDER INFO
        // ======================
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = null!;

        // ✅ ADD THIS (IMPORTANT)
        public DateTime? DeliveredDate { get; set; }

        // ======================
        // ITEMS & NESTED ORDERS
        // ======================
        public List<CustomerOrderItemDto> Items { get; set; } = new();
        public List<CustomerOrderDto> Orders { get; set; } = new();
    }
}
