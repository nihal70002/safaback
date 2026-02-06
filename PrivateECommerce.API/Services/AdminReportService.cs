using Microsoft.EntityFrameworkCore;
using PrivateECommerce.API.Data;
using PrivateECommerce.API.DTOs.Reports;

public class AdminReportService : IAdminReportService
{
    private readonly AppDbContext _context;

    public AdminReportService(AppDbContext context)
    {
        _context = context;
    }

    // 1️⃣ SALES SUMMARY
    public SalesSummaryDto GetSalesSummary()
    {
        var deliveredOrders = _context.Orders
            .Where(o => o.Status == "Delivered");

        var pendingOrders = _context.Orders
            .Where(o => o.Status != "Delivered" && o.Status != "Cancelled");

        var totalRevenue = deliveredOrders.Sum(o => o.TotalAmount);
        var pendingRevenue = pendingOrders.Sum(o => o.TotalAmount);

        var deliveredCount = deliveredOrders.Count();

        return new SalesSummaryDto
        {
            TotalRevenue = totalRevenue,
            PendingRevenue = pendingRevenue,
            AverageOrderValue = deliveredCount == 0 ? 0 : totalRevenue / deliveredCount
        };
    }

    // 2️⃣ MONTHLY SALES
    public IEnumerable<SalesTrendDto> GetMonthlySales(int year)
    {
        return _context.Orders
            .Where(o => o.Status == "Delivered" && o.OrderDate.Year == year)
            .GroupBy(o => o.OrderDate.Month)
            .Select(g => new SalesTrendDto
            {
                Period = g.Key.ToString(), // Month number
                Revenue = g.Sum(x => x.TotalAmount)
            })
            .OrderBy(x => x.Period)
            .ToList();
    }

    // 3️⃣ TOP SELLING PRODUCTS
    public IEnumerable<TopProductDto> GetTopProducts(int top = 5)
    {
        return _context.OrderItems
            .AsNoTracking()
            .Where(oi => oi.Order.Status == "Delivered")
            .Include(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.Product)
                    .ThenInclude(p => p.Images)
            .GroupBy(oi => oi.ProductVariant.Product)
            .Select(g => new TopProductDto
            {
                ProductName = g.Key.Name,

                ImageUrl = g.Key.Images
                    .OrderByDescending(i => i.IsPrimary)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault(),

                QuantitySold = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
            })
            .OrderByDescending(x => x.QuantitySold)
            .Take(top)
            .ToList();
    }


    public IEnumerable<CustomerProductInterestDto> GetCustomerProductInterest(int userId)
    {
        return _context.OrderItems
            .Where(oi => oi.Order.Status == "Delivered" &&
                         oi.Order.UserId == userId)
            .GroupBy(oi => oi.ProductVariant.Product.Name)
            .Select(g => new CustomerProductInterestDto
            {
                ProductName = g.Key,
                QuantityBought = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
            })
            .OrderByDescending(x => x.QuantityBought)
            .ToList();
    }
    public IEnumerable<TopCustomerDto> GetTopCustomers(int top = 5)
    {
        return _context.Orders
            .Where(o => o.Status == "Delivered")
            .GroupBy(o => o.User)
            .Select(g => new TopCustomerDto
            {
                UserId = g.Key.Id,
                CustomerName = g.Key.Name,
                Email = g.Key.Email,
                OrdersCount = g.Count(),
                TotalSpent = g.Sum(o => o.TotalAmount)
            })
            .OrderByDescending(x => x.TotalSpent)
            .Take(top)
            .ToList();
    }




}

