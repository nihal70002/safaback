using PrivateECommerce.API.Data;
using PrivateECommerce.API.Enum;
using Microsoft.EntityFrameworkCore;
using PrivateECommerce.API.Models;

namespace PrivateECommerce.API.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly AppDbContext _context;

        public WarehouseService(AppDbContext context)
        {
            _context = context;
        }

        // =============================
        // 📊 DASHBOARD – TODAY SUMMARY
        public object GetTodayOrderSummary()
        {
            var today = DateTime.UtcNow.Date;

            var orders = _context.Orders
                .Where(o => o.OrderDate.Date == today)
                .ToList();

            return new
            {
                TotalOrders = orders.Count,

                TotalAmount = orders.Sum(o => o.TotalAmount),

                Pending = orders.Count(o =>
                    o.Status == OrderStatus.PendingWarehouseApproval.ToString()
                ),

                Approved = orders.Count(o =>
                    o.Status == OrderStatus.Confirmed.ToString()
                ),

                Rejected = orders.Count(o =>
                    o.Status == OrderStatus.RejectedByWarehouse.ToString()
                )
            };
        }





        // =============================
        // 📜 DASHBOARD – TODAY ORDERS
        // =============================
        // =============================
        // 📜 DASHBOARD – TODAY ORDERS
        // =============================
        public IEnumerable<object> GetTodayOrders()
        {
            var today = DateTime.UtcNow.Date;

            return _context.Orders
                .Include(o => o.User)              // ✅ ADD THIS
                .Include(o => o.SalesExecutive)
                .Where(o => o.OrderDate.Date == today)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    Id = o.Id,
                    CreatedAt = o.OrderDate,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,

                    CustomerName = o.User.Name,        // ✅ ADD
                    CompanyName = o.User.CompanyName,  // ✅ ADD

                    SalesExecutive = o.SalesExecutive != null
                        ? o.SalesExecutive.Name
                        : "N/A"
                })
                .ToList();
        }
        public IEnumerable<object> GetAllOrders()
        {
            return _context.Orders
                .Include(o => o.User)
                .Include(o => o.SalesExecutive)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    Status = o.Status,

                    // ✅ THIS LINE FIXES YOUR ISSUE
                    TotalAmount = o.TotalAmount,

                    Customer = new
                    {
                        Name = o.User.Name,
                        Email = o.User.Email,
                        PhoneNumber = o.User.PhoneNumber
                    },

                    SalesExecutive = o.SalesExecutive == null ? null : new
                    {
                        o.SalesExecutive.Id,
                        o.SalesExecutive.Name
                    },

                    Items = o.OrderItems.Select(i => new
                    {
                        ProductName = i.ProductVariant.Product.Name,
                        VariantSize = i.ProductVariant.Size,
                        Quantity = i.Quantity
                    }).ToList(),

                    TotalQuantity = o.OrderItems.Sum(i => i.Quantity)
                })

                .ToList();
        }

        public object GetOrderDetails(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.User)
                .Include(o => o.SalesExecutive)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
                throw new Exception("Order not found");

            return new
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,

                CustomerName = order.User.Name,
                CompanyName = order.User.CompanyName,

                SalesExecutive = order.SalesExecutive != null
                    ? order.SalesExecutive.Name
                    : "N/A",

                Items = order.OrderItems.Select(i => new
                {
                    ProductName = i.ProductVariant.Product.Name,
                    Size = i.ProductVariant.Size,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.Quantity * i.UnitPrice
                })
            };
        }
        public void DispatchOrder(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order == null)
                throw new Exception("Order not found");

            if (order.Status != OrderStatus.Confirmed.ToString())
                throw new Exception("Only confirmed orders can be dispatched");

            order.Status = OrderStatus.Dispatched.ToString();

            _context.SaveChanges();
        }
       



        public void DeliverOrder(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order == null)
                throw new Exception("Order not found");

            if (order.Status != OrderStatus.Dispatched.ToString())
                throw new Exception("Only dispatched orders can be delivered");

            order.Status = OrderStatus.Delivered.ToString();
            order.DeliveredAt = DateTime.UtcNow; // ✅ FIELD EXISTS

            _context.SaveChanges();
        }



        // =============================
        // 📦 INVENTORY – ALL STOCK
        // =============================
        public IEnumerable<object> GetInventory()
        {
            return _context.ProductVariants
                .Include(pv => pv.Product)
                .Select(pv => new
                {
                    variantId = pv.Id,          // ✅ THIS WAS MISSING
                    productId = pv.Product.Id,
                    productName = pv.Product.Name,
                    size = pv.Size,
                    totalStock = pv.Stock,
                    availableStock = pv.Stock,
                    lowStockThreshold = pv.LowStockThreshold
                })
                .ToList();
        }


        public void ApproveOrder(int orderId, int userId, bool isAdmin = false)
        {
            var order = _context.Orders.Find(orderId)
                ?? throw new Exception("Order not found");

            if (order.Status != OrderStatus.PendingWarehouseApproval.ToString())
                throw new Exception("Order not pending warehouse approval");

            order.WarehouseApprovedAt = DateTime.UtcNow;
            order.WarehouseUserId = isAdmin ? null : userId;
            order.ApprovedByRole = isAdmin ? "Admin" : "Warehouse";

            order.Status = OrderStatus.Confirmed.ToString();

            _context.SaveChanges();
        }






        public void RejectOrder(int orderId, int warehouseUserId, string reason)
        {
            var order = _context.Orders.Find(orderId)
                ?? throw new Exception("Order not found");

            if (order.Status != OrderStatus.PendingWarehouseApproval.ToString())
                throw new Exception("Order is not pending warehouse approval");

            order.Status = OrderStatus.RejectedByWarehouse.ToString();
            order.WarehouseUserId = warehouseUserId;
            order.WarehouseRemarks = reason;

            // ✅ IMPORTANT: visible to admin
            order.RejectedReason = reason;

            _context.SaveChanges();
        }



        // =============================
        // 🔻 INVENTORY – LOW STOCK
        // =============================
        public IEnumerable<object> GetLowStockProducts(int threshold)
        {
            return _context.ProductVariants
                .Include(pv => pv.Product)
                .Where(pv => pv.Stock <= threshold)
                .Select(pv => new
                {
                    VariantId = pv.Id,          // ✅ ADD THIS
                    ProductId = pv.Product.Id,
                    ProductName = pv.Product.Name,
                    Size = pv.Size,
                    CurrentStock = pv.Stock,
                    Threshold = threshold
                })
                .ToList();
        }



        public IEnumerable<object> GetLowStockAlerts()
        {
            return _context.ProductVariants
                .Include(pv => pv.Product)
                .Where(pv => pv.Stock <= pv.LowStockThreshold)
                .OrderBy(pv => pv.Stock)
                .Select(pv => new
                {
                    VariantId = pv.Id,          // ✅ MUST
                    ProductId = pv.Product.Id,
                    ProductName = pv.Product.Name,
                    Size = pv.Size,
                    CurrentStock = pv.Stock,
                    Threshold = pv.LowStockThreshold,
                    Status = "LOW_STOCK"
                })
                .ToList();
        }


        public IEnumerable<object> GetStockMovements()
        {
            return _context.StockMovements
                .Include(s => s.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new
                {
                    ProductName = s.ProductVariant.Product.Name,
                    Size = s.ProductVariant.Size,
                    s.QuantityChanged,
                    s.MovementType,
                    s.Reason,
                    s.OrderId,
                    s.CreatedAt
                })
                .ToList();
        }


        // =============================
        // 👤 SALES EXECUTIVES – SUMMARY
        // =============================
        public IEnumerable<object> GetSalesExecutivesWithOrders()
        {
            return _context.Orders
                .Include(o => o.SalesExecutive)
                .Where(o => o.SalesExecutiveId != null)
                .GroupBy(o => new
                {
                    o.SalesExecutiveId,
                    o.SalesExecutive!.Name
                })
                .Select(g => new
                {
                    SalesExecutiveId = g.Key.SalesExecutiveId,
                    SalesExecutiveName = g.Key.Name,
                    TotalOrders = g.Count(),
                    PendingOrders = g.Count(o => o.Status.Contains("Pending"))
                })
                .ToList();
        }

        // =============================
        // 📦 SALES EXECUTIVE – ORDER HISTORY
        // =============================
        public IEnumerable<object> GetOrdersBySalesExecutive(
            int salesExecutiveId,
            string? status,
            DateTime? fromDate,
            DateTime? toDate
        )
        {
            var query = _context.Orders
                .Where(o => o.SalesExecutiveId == salesExecutiveId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(o => o.Status == status);

            if (fromDate.HasValue)
                query = query.Where(o => o.OrderDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(o => o.OrderDate <= toDate.Value);

            return query
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    o.Id,
                    o.OrderDate,
                    o.Status,
                    o.TotalAmount,
                    o.WarehouseApprovedAt,
                    o.DeliveredAt
                })
                .ToList();
        }
        public IEnumerable<object> GetLowStockAlerts(int threshold = 10)
        {
            return _context.ProductVariants
                .Include(pv => pv.Product)
                .Where(pv => pv.Stock <= threshold)
                .Select(pv => new
                {
                    ProductName = pv.Product.Name,
                    Size = pv.Size,
                    CurrentStock = pv.Stock,
                    Threshold = threshold
                })
                .ToList();
        }
        public IEnumerable<object> GetPendingOrders()
        {
            return _context.Orders
                .Include(o => o.User)
                .Include(o => o.SalesExecutive)
                .Where(o => o.Status == OrderStatus.PendingWarehouseApproval.ToString())
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,

                    CustomerName = o.User.Name,
                    CompanyName = o.User.CompanyName,

                    SalesExecutiveName = o.SalesExecutive != null
                        ? o.SalesExecutive.Name
                        : "N/A"
                })
                .ToList();
        }


    }
}
