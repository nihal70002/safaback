using System.Collections.Generic;
using PrivateECommerce.API.DTOs.Sales;

namespace PrivateECommerce.API.DTOs.Admin
{
    public class CustomerOrderDto
    {
        // Customer info
        public int CustomerId { get; set; }
        public string Name { get; set; } = null!;
        public string CompanyName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;

        // Summary
        public int TotalOrders { get; set; }
        public decimal TotalAmount { get; set; }

        // Order info
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = null!;

        // ✅ THIS IS THE MISSING PIECE
        public List<CustomerOrderItemDto> Items { get; set; } = new();
        public List<CustomerOrderDto> Orders { get; set; } = new();

    }
}
