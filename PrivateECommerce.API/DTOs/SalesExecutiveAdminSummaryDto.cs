namespace PrivateECommerce.API.DTOs.Admin
{
    public class SalesExecutiveAdminSummaryDto
    {
        public int SalesExecutiveId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int TotalCustomers { get; set; }

        public int TotalOrders { get; set; }
        public int AcceptedOrders { get; set; }
        public int RejectedOrders { get; set; }
        public int PendingOrders { get; set; }

        public decimal TotalOrderValue { get; set; }
    }
}
