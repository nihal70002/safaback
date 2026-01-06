using PrivateECommerce.API.Data;
using PrivateECommerce.API.DTOs;

namespace PrivateECommerce.API.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly AppDbContext _context;

        public AdminDashboardService(AppDbContext context)
        {
            _context = context;
        }

        public AdminDashboardSummaryDto GetSummary()
        {
            var today = DateTime.UtcNow.Date;

            return new AdminDashboardSummaryDto
            {
                TotalOrders = _context.Orders.Count(),

                PendingOrders = _context.Orders
                    .Count(o => o.Status == "Pending"),

                TodayOrders = _context.Orders
                    .Count(o => o.OrderDate.Date == today),

                TotalRevenue = _context.Orders
                    .Where(o => o.Status == "Delivered")
                    .Sum(o => (decimal?)o.TotalAmount) ?? 0,

                TodayRevenue = _context.Orders
                    .Where(o => o.Status == "Delivered" &&
                                o.OrderDate.Date == today)
                    .Sum(o => (decimal?)o.TotalAmount) ?? 0,

                ActiveProducts = _context.Products
                    .Count(p => p.IsActive),

                OutOfStockVariants = _context.ProductVariants
                    .Count(v => v.Stock <= 0)
            };
        }
    }
}
