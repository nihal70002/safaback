using Microsoft.EntityFrameworkCore;
using PrivateECommerce.API.Data;
using PrivateECommerce.API.DTOs;
using PrivateECommerce.API.DTOs.Admin;
using PrivateECommerce.API.DTOs.Sales;
using PrivateECommerce.API.Enum;


namespace PrivateECommerce.API.Services
{
    public class AdminSalesExecutiveService : IAdminSalesExecutiveService
    {
        private readonly AppDbContext _context;

        public AdminSalesExecutiveService(AppDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // CUSTOMERS WITH THEIR ORDERS (ORDER + DELIVERED DATE)
        // =====================================================
        public List<CustomerOrderDto> GetCustomersWithOrders(int salesExecutiveId)
        {
            var orders = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .Where(o => o.SalesExecutiveId == salesExecutiveId)
                .ToList();

            return orders
                .GroupBy(o => o.UserId)
                .Select(g =>
                {
                    var customer = g.First().User;

                    return new CustomerOrderDto
                    {
                        // =====================
                        // CUSTOMER INFO
                        // =====================
                        CustomerId = customer.Id,
                        Name = customer.Name,
                        CompanyName = customer.CompanyName,
                        Email = customer.Email,
                        PhoneNumber = customer.PhoneNumber,

                        TotalOrders = g.Count(),
                        TotalAmount = g.Sum(x => x.TotalAmount),

                        // =====================
                        // ORDERS (SAME DTO TYPE)
                        // =====================
                        Orders = g
                            .OrderByDescending(o => o.OrderDate)
                            .Select(o => new CustomerOrderDto
                            {
                                OrderId = o.Id,
                                OrderDate = o.OrderDate,
                                Status = o.Status.ToString(),
                                TotalAmount = o.TotalAmount,

                                Items = o.OrderItems.Select(i => new CustomerOrderItemDto
                                {
                                    ProductName = i.ProductVariant.Product.Name,
                                    Size = i.ProductVariant.Size,
                                    Quantity = i.Quantity,
                                    Price = i.ProductVariant.Price
                                }).ToList()
                            })
                            .ToList()
                    };
                })
                .ToList();
        }

        

        // =====================================================
        // SALES EXECUTIVE PERFORMANCE
        // =====================================================
        public SalesExecutivePerformanceDto GetSalesExecutivePerformance(int salesExecutiveId)
        {
            var salesExec = _context.Users
                .FirstOrDefault(u => u.Id == salesExecutiveId)
                ?? throw new Exception("Sales Executive not found");

            var orders = _context.Orders
                .Where(o => o.SalesExecutiveId == salesExecutiveId);

            var totalOrders = orders.Count();

            var pendingOrders = orders.Count(o =>
                o.SalesApprovedAt == null &&
                o.IsRejectedBySales == null
            );

            var acceptedOrders = orders.Count(o =>
                o.SalesApprovedAt != null &&
                o.IsRejectedBySales == null
            );

            var rejectedOrders = orders.Count(o =>
                o.IsRejectedBySales == true
            );

            var totalOrderValue = orders.Sum(o => o.TotalAmount);

            var lastOrderDate = orders
                .OrderByDescending(o => o.OrderDate)
                .Select(o => o.OrderDate)
                .FirstOrDefault();

            var customers = _context.Users
                .Where(u => u.SalesExecutiveId == salesExecutiveId);

            var totalCustomers = customers.Count();

            var activeCustomers = customers.Count(c =>
                _context.Orders.Any(o =>
                    o.UserId == c.Id &&
                    o.SalesExecutiveId == salesExecutiveId &&
                    o.Status != OrderStatus.Delivered.ToString() &&
                    o.Status != OrderStatus.Cancelled.ToString()
                )
            );

            return new SalesExecutivePerformanceDto
            {
                SalesExecutiveId = salesExec.Id,
                Name = salesExec.Name,

                TotalCustomers = totalCustomers,
                ActiveCustomers = activeCustomers,

                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                AcceptedOrders = acceptedOrders,
                RejectedOrders = rejectedOrders,

                TotalOrderValue = totalOrderValue,
                LastOrderDate = lastOrderDate
            };
        }

        // =====================================================
        // ORDERS BY STATUS
        // =====================================================
        public List<SalesExecutiveOrderDetailDto> GetOrdersByStatus(
            int salesExecutiveId,
            string type)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.ProductVariant)
                        .ThenInclude(v => v.Product)
                .Where(o => o.SalesExecutiveId == salesExecutiveId);

            query = (string.IsNullOrWhiteSpace(type) ? "total" : type.ToLower()) switch
            {
                "total" => query,

                "pending" => query.Where(o =>
                    o.SalesApprovedAt == null &&
                    o.IsRejectedBySales == null
                ),

                "accepted" => query.Where(o =>
                    o.SalesApprovedAt != null &&
                    o.IsRejectedBySales == null
                ),

                "completed" => query.Where(o =>
                    o.Status == OrderStatus.Delivered.ToString()
                ),

                "rejected" => query.Where(o =>
                    o.IsRejectedBySales == true
                ),

                _ => query
            };

            return query
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new SalesExecutiveOrderDetailDto
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,

                    Customer = new CustomerDetailDto
                    {
                        CustomerId = o.User.Id,
                        Name = o.User.Name,
                        CompanyName = o.User.CompanyName,
                        PhoneNumber = o.User.PhoneNumber,
                        Email = o.User.Email
                    },

                    Items = o.OrderItems.Select(i => new OrderItemDetailDto
                    {
                        ProductName = i.ProductVariant.Product.Name,
                        Size = i.ProductVariant.Size,
                        Quantity = i.Quantity
                    }).ToList()
                })
                .ToList();
        }

        // =====================================================
        // BASIC CUSTOMER LIST
        // =====================================================
        public List<CustomerBasicDto> GetCustomersForSalesExecutive(int salesExecutiveId)
        {
            return _context.Users
                .Where(u => u.SalesExecutiveId == salesExecutiveId)
                .Select(u => new CustomerBasicDto
                {
                    CustomerId = u.Id,
                    Name = u.Name,
                    CompanyName = u.CompanyName,
                    PhoneNumber = u.PhoneNumber,
                    Email = u.Email
                })
                .ToList();
        }
    }
}
