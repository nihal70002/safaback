using Microsoft.EntityFrameworkCore;
using PrivateECommerce.API.Data;
using PrivateECommerce.API.DTOs;
using PrivateECommerce.API.DTOs.Admin;
using PrivateECommerce.API.DTOs.Sales;
using PrivateECommerce.API.Enum;
using PrivateECommerce.API.Models;
using System.Net.Http.Headers;
using System.Text;


namespace PrivateECommerce.API.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public OrderService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ===========================
        // CUSTOMER
        // ===========================
       

        public async Task PlaceOrder(int customerId, PlaceOrderByCustomerDto dto)
        {
            Console.WriteLine("🟢 PlaceOrder started");

            var requestedVariantIds = dto.Items
                .Select(i => i.ProductVariantId)
                .ToList();

            var dbVariants = await _context.ProductVariants
                .Where(v => requestedVariantIds.Contains(v.Id))
                .ToDictionaryAsync(v => v.Id);

            await using var transaction = await _context.Database.BeginTransactionAsync();

            Order? order = null;

            try
            {
                var customer = await _context.Users
                    .Include(u => u.SalesExecutive)
                    .FirstOrDefaultAsync(u => u.Id == customerId);

                if (customer == null)
                    throw new Exception("Customer not found");

                order = new Order
                {
                    UserId = customerId,
                    SalesExecutiveId = customer.SalesExecutiveId,
                    Status = OrderStatus.PendingSalesApproval.ToString(),
                    OrderDate = DateTime.UtcNow,
                    OrderItems = new List<OrderItem>()
                };

                decimal totalAmount = 0;

                foreach (var item in dto.Items)
                {
                    if (!dbVariants.TryGetValue(item.ProductVariantId, out var variant))
                        throw new Exception($"Invalid product variant: {item.ProductVariantId}");

                    order.OrderItems.Add(new OrderItem
                    {
                        ProductVariantId = variant.Id,
                        Quantity = item.Quantity,
                        UnitPrice = variant.Price
                    });

                    totalAmount += variant.Price * item.Quantity;
                }

                order.TotalAmount = totalAmount;

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                Console.WriteLine($"✅ Order saved. OrderId: {order.Id}");
                Console.WriteLine($"SalesExecutiveId: {order.SalesExecutiveId}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"❌ Order failed: {ex.Message}");
                throw;
            }

            if (order != null)
            {
                try
                {
                    Console.WriteLine("📢 Calling SendNewOrderNotification");
                    await SendNewOrderNotification(order);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WhatsApp notification failed: {ex.Message}");
                }
            }
        }


       
private async Task SendNewOrderNotification(Order order)
        {
            Console.WriteLine("🔔 Fetching full order details for notification...");

            // Fetch full order with related data
            var fullOrder = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            if (fullOrder == null)
            {
                Console.WriteLine("❌ Order details not found in DB.");
                return;
            }

            // Build item list
            var itemsSummary = new StringBuilder();
            foreach (var item in fullOrder.OrderItems)
            {
                var name = item.ProductVariant?.Product?.Name ?? "Product";
                var variantDetails = item.ProductVariant?.Size ??
                                     item.ProductVariant?.Color ?? "";

                itemsSummary.AppendLine($"▫️ {item.Quantity} x {name} {variantDetails}");
            }

            // WhatsApp message
            var message = $"📦 *NEW ORDER RECEIVED*\n\n" +
                          $"🆔 *Order ID:* #{fullOrder.Id}\n" +
                          $"👤 *Customer:* {fullOrder.User?.Name}\n" +
                          $"🏢 *Company:* {fullOrder.User?.CompanyName ?? "N/A"}\n" +
                          $"📞 *Phone:* {fullOrder.User?.PhoneNumber}\n\n" +
                          $"🛒 *Items Ordered:*\n{itemsSummary}\n" +
                          $"💰 *Total Amount:* ₹{fullOrder.TotalAmount:N2}\n\n" +

                          $"🟢 *To approve this order reply:*\n" +
                          $"APPROVE-{fullOrder.Id}\n\n" +

                          $"❌ *To reject this order reply:*\n" +
                          $"REJECT-{fullOrder.Id}\n\n" +

                          $"👉 _Or review in the admin panel._";

            Console.WriteLine("📨 WhatsApp message prepared.");

            // Get Sales + Admin recipients
            var recipients = await _context.Users
                .Where(u => (u.Id == fullOrder.SalesExecutiveId || u.Role == "Admin")
                            && !string.IsNullOrEmpty(u.PhoneNumber))
                .Select(u => u.PhoneNumber)
                .ToListAsync();

            Console.WriteLine($"📱 Recipients found: {recipients.Count}");

            if (!recipients.Any())
            {
                Console.WriteLine("❌ No recipients found for this order.");
                return;
            }

            // Send WhatsApp
            foreach (var phone in recipients)
            {
                try
                {
                    Console.WriteLine($"➡ Sending notification to {phone}");

                    await SendWhatsapp(phone, message);

                    Console.WriteLine($"✅ Notification sent to {phone}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error sending to {phone}: {ex.Message}");
                }
            }

            Console.WriteLine("🏁 Order notification process finished.");
        }



        public async Task SendWhatsapp(string phone, string message)
        {
            // 1. Clean the phone number (Remove + if it exists to avoid + +91)
            var cleanPhone = phone.Replace("+", "").Trim();
            if (!cleanPhone.StartsWith("91")) cleanPhone = "91" + cleanPhone;

            var accountSid = _config["Twilio:AccountSid"];
            var authToken = _config["Twilio:AuthToken"];

            using var client = new HttpClient();
            var values = new Dictionary<string, string>
    {
        { "From", "whatsapp:+14155238886" }, // Ensure this matches your Sandbox/Live number
        { "To", $"whatsapp:+{cleanPhone}" },
        { "Body", message }
    };

            var content = new FormUrlEncodedContent(values);
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.twilio.com/2010-04-01/Accounts/{accountSid}/Messages.json");
            request.Content = content;

            var authBytes = Encoding.ASCII.GetBytes($"{accountSid}:{authToken}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));

            var response = await client.SendAsync(request);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // This will tell you EXACTLY why it failed (e.g., "Not a valid sandbox user")
                Console.WriteLine($"❌ Twilio API Error: {response.StatusCode} - {responseText}");
            }
        }


        public IEnumerable<UserOrderListDto> GetOrdersForUser(int userId)
        {
            return _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new UserOrderListDto
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount
                }).ToList();
        }

        public UserOrderDetailDto GetOrderForUser(int orderId, int userId)
        {
            var order = _context.Orders
                .FirstOrDefault(o => o.Id == orderId && o.UserId == userId)
                ?? throw new Exception("Order not found");

            return new UserOrderDetailDto
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount
            };
        }
        public List<SalesOrderListDto> GetPendingOrdersForSales(int salesExecutiveId)
        {
            return _context.Orders
                .Where(o =>
                    o.SalesExecutiveId == salesExecutiveId &&
                    o.Status == OrderStatus.PendingSalesApproval.ToString()
                )
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .Select(o => new SalesOrderListDto
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    RejectedReason = o.RejectedReason,

                    Customer = new CustomerDto
                    {
                        Name = o.User.Name,
                        CompanyName = o.User.CompanyName
                    },
                    Items = o.OrderItems.Select(i => new SalesOrderItemDto
                    {
                        ProductName = i.ProductVariant.Product.Name,
                        Quantity = i.Quantity
                    }).ToList()
                })
                .ToList();
        }

        // ===========================
        // SALES EXECUTIVE
        // ===========================
        public IEnumerable<object> GetPendingOrdersForWarehouse()
        {
            return _context.Orders
                .Where(o => o.Status == OrderStatus.PendingWarehouseApproval.ToString())
                .Include(o => o.User)
                .Include(o => o.SalesExecutive)
                .Select(o => new
                {
                    OrderId = o.Id,
                    o.OrderDate,
                    o.TotalAmount,

                    CustomerName = o.User.Name,
                    CompanyName = o.User.CompanyName,

                    SalesExecutiveName = o.SalesExecutive.Name
                })
                .ToList();
        }


        public List<SalesOrderListDto> GetOrdersForSalesExecutive(int salesExecutiveId)
        {
            return _context.Orders
                .Where(o => o.SalesExecutiveId == salesExecutiveId)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new SalesOrderListDto
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    RejectedReason = o.RejectedReason,

                    Customer = new CustomerDto
                    {
                        Name = o.User.Name,
                        CompanyName = o.User.CompanyName,
                        PhoneNumber = o.User.PhoneNumber
                    },

                    Items = o.OrderItems.Select(i => new SalesOrderItemDto
                    {
                        ProductName = i.ProductVariant.Product.Name,
                        Size = i.ProductVariant.Size,

                        Class = i.ProductVariant.Class,
                        Style = i.ProductVariant.Style,
                        Material = i.ProductVariant.Material,
                        Color = i.ProductVariant.Color,

                        Quantity = i.Quantity,
                        ProductCode = i.ProductVariant.ProductCode,
                        UnitPrice = i.UnitPrice
                    }).ToList()


                })
                .ToList();
        }




        public async Task<Order> ApproveBySales(int orderId, int approverUserId, bool isAdmin = false)
        {
            Console.WriteLine($"🔎 Starting Approval for Order: {orderId}");

            // 1. Fetch the order
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) throw new Exception("Order not found");

            // 2. Status Check
            string currentStatus = order.Status.ToString();
            if (!string.Equals(currentStatus, OrderStatus.PendingSalesApproval.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception($"Invalid Status: Order is currently '{currentStatus}'");
            }

            // 3. Apply Changes
            if (!isAdmin && approverUserId > 0)
            {
                order.SalesExecutiveId = approverUserId;
            }
            order.SalesApprovedAt = DateTime.UtcNow;
            order.ApprovedByRole = isAdmin ? "Admin" : "Sales";

            // Set the new status
            order.Status = OrderStatus.PendingWarehouseApproval.ToString();

            // 4. Force Entity Framework to track these changes
            _context.Entry(order).State = EntityState.Modified;

            // 5. Save and Verify
            int rowsAffected = await _context.SaveChangesAsync();
            Console.WriteLine($"💾 Rows updated in DB: {rowsAffected}");

            // 6. Double-check from the DB directly (No-Tracking)
            var checkDb = await _context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (checkDb.Status != OrderStatus.PendingWarehouseApproval.ToString())
            {
                Console.WriteLine("⚠️ CRITICAL: SaveChanges succeeded but the Status in DB did not change!");
                throw new Exception("Database update failed to persist. Please check database constraints.");
            }

            // 7. Notification (Background)
            try
            {
                await SendWarehouseNotification(order);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WhatsApp Failed: {ex.Message}");
            }

            return order;
        }

        private async Task SendWarehouseNotification(Order order)
        {
            Console.WriteLine("📦 SendWarehouseNotification triggered");

            var dbOrder = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.ProductVariant)
                .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            if (dbOrder == null)
            {
                Console.WriteLine("❌ Order not found for warehouse notification");
                return;
            }
              

            var phones = await _context.Users
                .Where(u => u.Role == "Warehouse" && !string.IsNullOrEmpty(u.PhoneNumber))
                .Select(u => u.PhoneNumber)
                .ToListAsync();

            Console.WriteLine($"📱 Warehouse phones found: {phones.Count}");

            if (!phones.Any())
            {
                Console.WriteLine("❌ No warehouse phone numbers found.");
                return;
            }

            var items = string.Join("\n", dbOrder.OrderItems.Select(i =>
                $"{i.Quantity} x {i.ProductVariant.Product.Name}"
            ));

            var message =
        $@"📦 Order Approved By Sales

Order ID: {dbOrder.Id}

Items:
{items}

Total Amount: ₹{dbOrder.TotalAmount}";

            foreach (var phone in phones)
            {
                Console.WriteLine($"➡ Sending to warehouse: {phone}");
                await SendWhatsapp(phone, message);
            }

            Console.WriteLine("🏁 Warehouse notification completed");
        }

        public async Task RejectBySales(int orderId, int salesId, string reason)
        {
            Console.WriteLine($"❌ Rejecting Order {orderId}");

            var order = await _context.Orders.FindAsync(orderId)
                ?? throw new Exception("Order not found");

            if (order.Status != OrderStatus.PendingSalesApproval.ToString())
                throw new Exception($"Cannot reject order in '{order.Status}' status.");

            // Only update SalesExecutiveId if valid
            if (salesId > 0)
                order.SalesExecutiveId = salesId;

            order.Status = OrderStatus.RejectedBySales.ToString();
            order.IsRejectedBySales = true;
            order.RejectedReason = reason;
            order.SalesApprovedAt = null;

            await _context.SaveChangesAsync();

            Console.WriteLine($"❌ Order {orderId} rejected successfully with reason: {reason}");
        }


        public CustomerOrderDto GetCustomerOrderHistory(int salesExecutiveId, int customerId)
        {
            var orders = _context.Orders
                .Where(o =>
                    o.SalesExecutiveId == salesExecutiveId &&
                    o.UserId == customerId)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            // 🔹 Customer exists but no orders
            if (!orders.Any())
            {
                var customer = _context.Users.FirstOrDefault(u =>
                    u.Id == customerId &&
                    u.SalesExecutiveId == salesExecutiveId);

                if (customer == null)
                    throw new Exception("Customer not found");

                return new CustomerOrderDto
                {
                    CustomerId = customer.Id,
                    Name = customer.Name,
                    CompanyName = customer.CompanyName,
                    Email = customer.Email,
                    PhoneNumber = customer.PhoneNumber,

                    TotalOrders = 0,
                    TotalAmount = 0,
                    Orders = new List<CustomerOrderDto>()
                };
            }

            var firstOrderCustomer = orders.First().User;

            return new CustomerOrderDto
            {
                // ======================
                // CUSTOMER INFO
                // ======================
                CustomerId = firstOrderCustomer.Id,
                Name = firstOrderCustomer.Name,
                CompanyName = firstOrderCustomer.CompanyName,
                Email = firstOrderCustomer.Email,
                PhoneNumber = firstOrderCustomer.PhoneNumber,

                // ======================
                // SUMMARY
                // ======================
                TotalOrders = orders.Count,
                TotalAmount = orders.Sum(o => o.TotalAmount),

                // ======================
                // ORDERS LIST
                // ======================
                Orders = orders.Select(o => new CustomerOrderDto
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    Status = o.Status.ToString(),
                    TotalAmount = o.TotalAmount,

                    // ✅ THIS IS THE FIX
                    DeliveredDate = o.DeliveredAt,

                    Items = o.OrderItems.Select(i => new CustomerOrderItemDto
                    {
                        ProductName = i.ProductVariant.Product.Name,
                        Size = i.ProductVariant.Size,

                        Class = i.ProductVariant.Class,
                        Style = i.ProductVariant.Style,
                        Material = i.ProductVariant.Material,
                        Color = i.ProductVariant.Color,

                        Quantity = i.Quantity,
                        Price = i.UnitPrice,
                        ProductCode = i.ProductVariant.ProductCode
                    }).ToList()

                }).ToList()
            };
        }





        // ===========================
        // ADMIN
        // ===========================


        public void AcceptOrder(int orderId, int salesExecutiveId)
        {
            var order = _context.Orders.Find(orderId) ?? throw new Exception("Order not found");
            order.Status = OrderStatus.Confirmed.ToString();
            order.AdminApprovedAt = DateTime.UtcNow;
            _context.SaveChanges();
        }

        public void CancelOrderByAdmin(int orderId, string reason)
        {
            var order = _context.Orders.Find(orderId)
                ?? throw new Exception("Order not found");

            order.Status = OrderStatus.Cancelled.ToString();
            order.RejectedReason = reason; // rename to AdminRemark if you want
            _context.SaveChanges();
        }


        

        public void ConfirmOrder(int orderId)
        {
            var order = _context.Orders.Find(orderId) ?? throw new Exception("Order not found");
            order.Status = OrderStatus.Confirmed.ToString();
            order.AdminApprovedAt = DateTime.UtcNow;
            _context.SaveChanges();
        }

        public void DispatchOrder(int orderId)
        {
            var order = _context.Orders.Find(orderId) ?? throw new Exception("Order not found");
            order.Status = OrderStatus.Dispatched.ToString();
            _context.SaveChanges();
        }

        public void DeliverOrder(int orderId)
        {
            var order = _context.Orders.Find(orderId)
                ?? throw new Exception("Order not found");

            // set delivered date only once
            if (order.DeliveredAt == null)
            {
                order.DeliveredAt = DateTime.UtcNow;
            }

            order.Status = OrderStatus.Delivered.ToString();
            _context.SaveChanges();
        }

        public void CancelOrder(int orderId, string reason)
        {
            var order = _context.Orders.Find(orderId)
                ?? throw new Exception("Order not found");

            order.Status = OrderStatus.Cancelled.ToString();
            order.IsRejectedBySales = true;
            order.RejectedReason = reason;
            order.SalesApprovedAt = null;

            _context.SaveChanges();
        }



        public void RevertOrderStatus(int orderId, bool isConfirmed)
        {
            var order = _context.Orders.Find(orderId)
                ?? throw new Exception("Order not found");

            // 🔒 Require confirmation for high-impact states
            if (
                (order.Status == OrderStatus.Delivered.ToString() ||
                 order.Status == OrderStatus.Dispatched.ToString())
                && !isConfirmed
            )
            {
                throw new Exception("CONFIRMATION_REQUIRED");
            }

            if (order.Status == OrderStatus.Delivered.ToString())
            {
                order.Status = OrderStatus.Dispatched.ToString();
                order.DeliveredAt = null;
            }
            else if (order.Status == OrderStatus.Dispatched.ToString())
            {
                order.Status = OrderStatus.Confirmed.ToString();
                order.DispatchedAt = null;
            }
            else if (order.Status == OrderStatus.Confirmed.ToString())
            {
                order.Status = OrderStatus.PendingWarehouseApproval.ToString();
                order.AdminApprovedAt = null;
            }
            else if (order.Status == OrderStatus.Cancelled.ToString())
            {
                // 🔁 Sales-rejected → back to Sales/Warehouse flow
                order.Status = OrderStatus.PendingWarehouseApproval.ToString();
                order.RejectedReason = null;
            }
            else
            {
                throw new Exception($"Revert not allowed from status '{order.Status}'");
            }

            _context.SaveChanges();
        }


        public async Task<OrderDetailsDto?> GetMyOrderDetailsAsync(int userId, int orderId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.Id == orderId && o.UserId == userId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                            .ThenInclude(p => p.Images)
                .Select(o => new OrderDetailsDto
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    DeliveredDate = o.DeliveredAt,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    Items = o.OrderItems.Select(i => new OrderItemDto
                    {
                        ProductId = i.ProductVariant.Product.Id,
                        ProductName = i.ProductVariant.Product.Name,
                        ProductVariantId = i.ProductVariantId,

                        Size = i.ProductVariant.Size,
                        Class = i.ProductVariant.Class,
                        Style = i.ProductVariant.Style,
                        Material = i.ProductVariant.Material,
                        Color = i.ProductVariant.Color,

                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        Subtotal = i.UnitPrice * i.Quantity,

                        ProductImage = i.ProductVariant.Product.Images
                            .OrderByDescending(img => img.IsPrimary)
                            .Select(img => img.ImageUrl)
                            .FirstOrDefault()
                    })
.ToList()
                })
                .FirstOrDefaultAsync();
        }




        public AdminOrderDetailDto GetOrderById(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.User)
                .Include(o => o.SalesExecutive)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .FirstOrDefault(o => o.Id == orderId)
                ?? throw new Exception("Order not found");

            return new AdminOrderDetailDto
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,

                // CUSTOMER
                CustomerName = order.User.Name,
                CompanyName = order.User.CompanyName,
                PhoneNumber = order.User.PhoneNumber,

                // SALES EXECUTIVE
                SalesExecutiveId = order.SalesExecutiveId,
                SalesExecutiveName = order.SalesExecutive?.Name,
                SalesExecutivePhone = order.SalesExecutive?.PhoneNumber,

                // ✅ THIS WAS MISSING
                RejectedReason = order.RejectedReason,

                Items = order.OrderItems.Select(i => new AdminOrderItemDto
                {
                    ProductName = i.ProductVariant.Product.Name,
                    Size = i.ProductVariant.Size,

                    Class = i.ProductVariant.Class,
                    Style = i.ProductVariant.Style,
                    Material = i.ProductVariant.Material,
                    Color = i.ProductVariant.Color,

                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()

            };
        }



        public IEnumerable<AdminOrderListDto> GetRecentOrders(int count)
        {
            return _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .Select(o => new AdminOrderListDto
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,

                    CustomerName = o.User.Name,
                    CompanyName = o.User.CompanyName,
                    PhoneNumber = o.User.PhoneNumber
                })
                .ToList();
        }




        public async Task UpdateOrderByCustomer(
    int orderId,
    int userId,
    PlaceOrderByCustomerDto dto)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
                throw new Exception("Order not found");

            if (order.Status != OrderStatus.PendingSalesApproval.ToString())
                throw new Exception("Order cannot be edited after approval");

            _context.OrderItems.RemoveRange(order.OrderItems);

            decimal totalAmount = 0;

            foreach (var item in dto.Items)
            {
                var variant = await _context.ProductVariants
                    .FirstOrDefaultAsync(v => v.Id == item.ProductVariantId);

                if (variant == null)
                    throw new Exception("Invalid variant");

                order.OrderItems.Add(new OrderItem
                {
                    ProductVariantId = variant.Id,
                    Quantity = item.Quantity,
                    UnitPrice = variant.Price
                });

                totalAmount += variant.Price * item.Quantity;
            }

            order.TotalAmount = totalAmount;

            await _context.SaveChangesAsync();
        }





        public async Task CancelOrderByCustomer(int orderId, int userId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
                throw new Exception("Order not found");

            if (order.Status != "PendingSalesApproval")
                throw new Exception("Order cannot be cancelled at this stage");

            order.Status = "Cancelled";

            await _context.SaveChangesAsync();
        }




        public PagedResponseDto<AdminOrderListDto> GetFilteredOrders(
      string? status,
      bool today,
      int page,
      int pageSize)
        {
            var query = _context.Orders
                .Include(o => o.User)   // ✅ REQUIRED
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(o => o.Status == status);

            if (today)
            {
                var todayDate = DateTime.UtcNow.Date;
                query = query.Where(o => o.OrderDate.Date == todayDate);
            }

            var total = query.Count();

            var items = query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new AdminOrderListDto
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,

                    CustomerName = o.User.Name,
                    CompanyName = o.User.CompanyName,
                    PhoneNumber = o.User.PhoneNumber,
                    IsRejectedBySales = o.IsRejectedBySales
                })
                .ToList();

            return new PagedResponseDto<AdminOrderListDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

    }
}