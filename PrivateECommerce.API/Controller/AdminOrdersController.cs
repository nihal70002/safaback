using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivateECommerce.API.Services;

namespace PrivateECommerce.API.Controllers
{
    [ApiController]
    [Route("api/admin/orders")]
    [Authorize(Roles = "Admin")] // 🔐 ADMIN ONLY
    public class AdminOrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public AdminOrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // ==========================
        // 1️⃣ GET ALL ORDERS (ADMIN)
        // ==========================
        [HttpGet]
        public IActionResult GetAllOrders()
        {
            var orders = _orderService.GetAllOrders();
            return Ok(orders);
        }

        // ==========================
        // 2️⃣ GET ORDER DETAILS
        // ==========================
        [HttpGet("{orderId}")]
        public IActionResult GetOrderById(int orderId)
        {
            var order = _orderService.GetOrderById(orderId);

            if (order == null)
                return NotFound("Order not found");

            return Ok(order);
        }

        // ==========================
        // 3️⃣ CONFIRM ORDER
        // ==========================
        [HttpPut("{orderId}/confirm")]
        public IActionResult ConfirmOrder(int orderId)
        {
            _orderService.ConfirmOrder(orderId);
            return Ok("Order confirmed");
        }

        // ==========================
        // 4️⃣ DISPATCH ORDER
        // ==========================
        [HttpPut("{orderId}/dispatch")]
        public IActionResult DispatchOrder(int orderId)
        {
            _orderService.DispatchOrder(orderId);
            return Ok("Order dispatched");
        }

        // ==========================
        // 5️⃣ DELIVER ORDER
        // ==========================
        [HttpPut("{orderId}/deliver")]
        public IActionResult DeliverOrder(int orderId)
        {
            _orderService.DeliverOrder(orderId);
            return Ok("Order delivered");
        }
        [HttpGet("recent")]
        public IActionResult GetRecentOrders()
        {
            return Ok(_orderService.GetRecentOrders(10));
        }


        // ==========================
        // 6️⃣ CANCEL ORDER
        // ==========================
        [HttpPut("{orderId}/cancel")]
        public IActionResult CancelOrder(int orderId)
        {
            _orderService.CancelOrder(orderId);
            return Ok("Order cancelled");
        }

        [HttpGet("filter")]
        public IActionResult FilterOrders(
    [FromQuery] string? status,
    [FromQuery] bool today = false)
        {
            return Ok(_orderService.GetFilteredOrders(status, today));
        }

    }
}
