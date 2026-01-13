using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivateECommerce.API.DTOs;
using PrivateECommerce.API.Services;
using System.Security.Claims;

namespace PrivateECommerce.API.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // ==========================
        // 1️⃣ PLACE ORDER (USER)
        // ==========================
        [HttpPost]
        public IActionResult PlaceOrder(PlaceOrderDto dto)
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            try
            {
                _orderService.PlaceOrder(userId, dto);
                return Ok(new { message = "Order placed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ==========================
        // 2️⃣ USER ORDER HISTORY
        // ==========================
        [HttpGet("my")]
        public IActionResult GetMyOrders()
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var orders = _orderService.GetOrdersForUser(userId);
            return Ok(orders);
        }

        // ==========================
        // 3️⃣ USER ORDER DETAILS
        // ==========================
        [HttpGet("my/{orderId}")]
        public IActionResult GetMyOrderDetails(int orderId)
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var order = _orderService.GetOrderForUser(orderId, userId);

            if (order == null)
                return NotFound(new { message = "Order not found" });

            return Ok(order);
        }
    }
}
