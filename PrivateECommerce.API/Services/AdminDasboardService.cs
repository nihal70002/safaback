using PrivateECommerce.API.Data;
using PrivateECommerce.API.DTOs.Admin;
using PrivateECommerce.API.Enum;

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
                // 📦 All orders
                TotalOrders = _context.Orders.Count(),

                // 🔴 Pending – waiting for SALES
                PendingSalesApproval = _context.Orders
                    .Count(o => o.Status == nameof(OrderStatus.PendingSalesApproval)),

                // 🟠 Pending – waiting for ADMIN
                PendingAdminApproval = _context.Orders
                    .Count(o => o.Status == nameof(OrderStatus.PendingAdminApproval)),

                // 📅 Orders created today
                TodayOrders = _context.Orders
                    .Count(o => o.OrderDate.Date == today),

                // 💰 Revenue ONLY from delivered orders
                TotalRevenue = _context.Orders
                    .Where(o => o.Status == nameof(OrderStatus.Delivered))
                    .Sum(o => (decimal?)o.TotalAmount) ?? 0,

                // 💰 Today revenue
                TodayRevenue = _context.Orders
                    .Where(o => o.Status == nameof(OrderStatus.Delivered) &&
                                o.OrderDate.Date == today)
                    .Sum(o => (decimal?)o.TotalAmount) ?? 0,

                // 📦 Products
                ActiveProducts = _context.Products
                    .Count(p => p.IsActive),

                // ⚠️ Stock alerts
                OutOfStockVariants = _context.ProductVariants
                    .Count(v => v.Stock <= 0)
            };
        }
    }
}
