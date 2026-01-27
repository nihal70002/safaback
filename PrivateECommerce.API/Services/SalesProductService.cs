using Microsoft.EntityFrameworkCore;
using PrivateECommerce.API.Data;
using PrivateECommerce.API.DTOs;
using PrivateECommerce.API.Enum;

public class SalesProductService : ISalesProductService
{
    private readonly AppDbContext _context;

    public SalesProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SalesProductPerformanceDto>> GetProductPerformanceAsync(int salesExecutiveId)
    {
        var validStatuses = new HashSet<string>
        {
            OrderStatus.PendingAdminApproval.ToString(),
            OrderStatus.Delivered.ToString()
        };

        return await _context.Orders
            .Where(o =>
                o.SalesExecutiveId == salesExecutiveId &&
                validStatuses.Contains(o.Status)
            )
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                    .ThenInclude(pv => pv.Product)
            .SelectMany(o => o.OrderItems)
            .GroupBy(oi => new
            {
                oi.ProductVariant.ProductId,
                ProductName = oi.ProductVariant.Product.Name,
                Price = oi.ProductVariant.Price   // ✅ FIX HERE
            })
            .Select(g => new SalesProductPerformanceDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.ProductName,
                Price = g.Key.Price,
                QuantitySold = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.Quantity * g.Key.Price)
            })
            .OrderByDescending(x => x.QuantitySold)
            .ToListAsync();
    }
}
