namespace PrivateECommerce.API.DTOs
{
    public class AdminDashboardSummaryDto
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int TodayOrders { get; set; }

        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }

        public int ActiveProducts { get; set; }
        public int OutOfStockVariants { get; set; }
    }
}
