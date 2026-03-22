namespace PrivateECommerce.API.DTOs.Admin
{
    public class AdminDashboardSummaryDto
    {
        public int TotalOrders { get; set; }

        // ✅ NEW (CORRECT)
        public int PendingSalesApproval { get; set; }
        public int PendingAdminApproval { get; set; }

        public int TodayOrders { get; set; }

        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }

        public int ActiveProducts { get; set; }
        public int OutOfStockVariants { get; set; }
    }
}
