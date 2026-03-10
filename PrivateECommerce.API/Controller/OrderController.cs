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
        // CUSTOMER: PLACE ORDER
        // ==========================
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(PlaceOrderByCustomerDto dto)
        {
            try
            {
                int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                await _orderService.PlaceOrder(userId, dto);

                return Ok("Order placed successfully");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("my/{orderId}")]
        public async Task<IActionResult> GetMyOrderDetails(int orderId)
        {
            var userId = int.Parse(
    User.FindFirstValue(ClaimTypes.NameIdentifier)
);

            // from JWT claim

            var order = await _orderService.GetMyOrderDetailsAsync(userId, orderId);

            if (order == null)
                return NotFound("Order not found");

            return Ok(order);
        }

        // ==========================
        // CUSTOMER: MY ORDERS
        // ==========================
        [HttpGet("my")]
        [Authorize(Roles = "Customer")]
        public IActionResult GetMyOrders()
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            return Ok(_orderService.GetOrdersForUser(userId));
        }

        // ==========================
        // SALES EXECUTIVE: PENDING ORDERS
        // ==========================
        [HttpGet("sales/pending")]
        [Authorize(Roles = "SalesExecutive")]
        public IActionResult GetPendingOrdersForSales()
        {
            int salesId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            return Ok(_orderService.GetPendingOrdersForSales(salesId));
        }

        // ==========================
        // SALES EXECUTIVE: APPROVE ORDER
        // ==========================
        [HttpPost("sales/{orderId}/approve")]
        [Authorize(Roles = "SalesExecutive")]
        public IActionResult ApproveOrderBySales(int orderId)
        {
            int salesId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            _orderService.ApproveBySales(orderId, salesId);
            return Ok(new { message = "Order approved successfully" });
        }

        // ==========================
        // SALES EXECUTIVE: REJECT ORDER
        // ==========================
        [HttpPost("sales/{orderId}/reject")]
        [Authorize(Roles = "SalesExecutive")]
        public IActionResult RejectOrderBySales(int orderId)
        {
            int salesId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            _orderService.RejectBySales(orderId, salesId);
            return Ok(new { message = "Order rejected successfully" });
        }
    }
}
