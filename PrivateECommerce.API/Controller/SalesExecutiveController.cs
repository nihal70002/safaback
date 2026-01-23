using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivateECommerce.API.DTOs;
using PrivateECommerce.API.DTOs.Orders;
using PrivateECommerce.API.Enum;
using PrivateECommerce.API.Services;
using System.Security.Claims;

namespace PrivateECommerce.API.Controllers
{
    [ApiController]
    [Route("api/sales")]
    [Authorize(Roles = "SalesExecutive")]
    public class SalesExecutiveController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;

        public SalesExecutiveController(
            IUserService userService,
            IOrderService orderService)
        {
            _userService = userService;
            _orderService = orderService;
        }

        // ===========================
        // SAFE SALES EXECUTIVE ID
        // ===========================
        private int SalesExecutiveId
        {
            get
            {
                var idClaim =
                    User.FindFirst(ClaimTypes.NameIdentifier) ??
                    User.FindFirst("id") ??
                    User.FindFirst("userId") ??
                    User.FindFirst("sub");

                if (idClaim == null || !int.TryParse(idClaim.Value, out var salesId))
                    throw new UnauthorizedAccessException("Invalid SalesExecutive ID");

                return salesId;
            }
        }

        // ===========================
        // 1️⃣ CREATE CUSTOMER
        // ===========================
        [HttpPost("customers")]
        public IActionResult CreateCustomer(CreateCustomerBySalesDto dto)
        {
            _userService.CreateCustomerBySales(dto, SalesExecutiveId);
            return Ok(new { message = "Customer created successfully" });
        }

        // ===========================
        // 2️⃣ MY CUSTOMERS
        // ===========================
        [HttpGet("customers")]
        public IActionResult GetMyCustomers()
        {
            var customers = _userService.GetCustomersForSalesExecutive(SalesExecutiveId);
            return Ok(customers);
        }

        // ===========================
        // 3️⃣ ALL MY ORDERS
        // ===========================
        [HttpGet("orders")]
        public IActionResult GetMyOrders()
        {
            var orders = _orderService.GetOrdersForSalesExecutive(SalesExecutiveId);
            return Ok(orders);
        }

        // ===========================
        // 4️⃣ ORDERS BY TYPE
        // ===========================
        // type = pending | accepted | completed | rejected
        [HttpGet("orders/filter")]
        public IActionResult GetOrdersByType([FromQuery] string type)
        {
            var orders = _orderService.GetOrdersForSalesExecutive(SalesExecutiveId);

            var filtered = type.ToLower() switch
            {
                "pending" => orders.Where(o =>
                    o.SalesApprovedAt == null &&
                    o.AdminApprovedAt == null
                ),

                "accepted" => orders.Where(o =>
                    o.SalesApprovedAt != null &&
                    o.Status == OrderStatus.PendingAdminApproval.ToString()
                ),

                "completed" => orders.Where(o =>
                    o.Status == OrderStatus.Delivered.ToString()
                ),

                "rejected" => orders.Where(o =>
                    o.Status == OrderStatus.RejectedBySales.ToString()
                ),

                _ => throw new Exception("Invalid order type")
            };

            return Ok(filtered);
        }

        // ===========================
        // 5️⃣ PENDING ORDERS (OLD – KEEP)
        // ===========================
        [HttpGet("orders/pending")]
        public IActionResult GetPendingOrders()
        {
            var orders = _orderService.GetPendingOrdersForSales(SalesExecutiveId);
            return Ok(orders);
        }

        // ===========================
        // 6️⃣ APPROVE ORDER
        // ===========================
        [HttpPut("orders/{orderId}/approve")]
        public IActionResult ApproveOrder(int orderId)
        {
            _orderService.ApproveBySales(orderId, SalesExecutiveId);
            return Ok(new { message = "Order approved successfully" });
        }

        // ===========================
        // 7️⃣ REJECT ORDER
        [HttpPost("cancel/{orderId}")]
        public IActionResult CancelOrder(
    int orderId,
    [FromBody] RejectOrderDto dto
)
        {
            _orderService.CancelOrder(orderId, dto.Reason);
            return Ok("Order cancelled by sales");
        }



        // =====================================================
        // 8️⃣ CUSTOMER → FULL ORDER HISTORY (🔥 NEW API)
        // =====================================================
        // ===========================
        // 8️⃣ CUSTOMER → FULL ORDER HISTORY
        // ===========================
        [HttpGet("customers/{customerId}/orders")]
        public IActionResult GetCustomerOrderHistory(int customerId)
        {
            var orders = _orderService.GetCustomerOrderHistory(
                SalesExecutiveId,
                customerId
            );

            // ✅ Empty list is valid response
            return Ok(orders);
        }
    }

    }

