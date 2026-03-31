using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivateECommerce.API.DTOs;
using PrivateECommerce.API.DTOs.Orders;
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



        [HttpPut("{id}/edit")]
        public async Task<IActionResult> EditOrder(int id, PlaceOrderByCustomerDto dto)
        {
            int userId = int.Parse(
    User.FindFirstValue(ClaimTypes.NameIdentifier)!
);

            await _orderService.UpdateOrderByCustomer(id, userId, dto);

            return Ok("Order updated");
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

        [HttpPut("sales/orders/{orderId}/approve")]
        [Authorize(Roles = "SalesExecutive")]
        
        public async Task<IActionResult> ApproveOrderBySales(int orderId)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

                int salesId = int.Parse(userIdClaim);

                // This call MUST be awaited
                var order = await _orderService.ApproveBySales(orderId, salesId, false);

                // If we reach here, it means the service didn't throw an exception
                return Ok(new
                {
                    message = "Order approved successfully",
                    orderId = order.Id,
                    status = order.Status
                });
            }
            catch (Exception ex)
            {
                // This is where 'Order not found' will end up
                return BadRequest(new { message = ex.Message });
            }
        }


        // ==========================
        // SALES EXECUTIVE: REJECT ORDER
        // ==========================
        [HttpPost("cancel/{orderId}")]
        [Authorize(Roles = "SalesExecutive")]
        public async Task<IActionResult> CancelOrder(int orderId, [FromBody] RejectOrderDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized();

                int salesId = int.Parse(userIdClaim);

                await _orderService.RejectBySales(orderId, salesId, dto.Reason);

                return Ok(new
                {
                    message = "Order rejected by sales",
                    orderId = orderId,
                    reason = dto.Reason
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
