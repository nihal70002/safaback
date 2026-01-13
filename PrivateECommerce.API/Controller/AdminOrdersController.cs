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

        [HttpGet]
        public IActionResult GetOrders(
     [FromQuery] string? status,
     [FromQuery] bool today = false,
     [FromQuery] int page = 1,
     [FromQuery] int pageSize = 10
 )
        {
            var result = _orderService.GetFilteredOrders(status, today, page, pageSize);
            return Ok(result);
        }


        // ==========================
        // GET ORDER DETAILS
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
        // CONFIRM ORDER
        // ==========================
        [HttpPut("{orderId}/confirm")]
        public IActionResult ConfirmOrder(int orderId)
        {
            _orderService.ConfirmOrder(orderId);
            return Ok("Order confirmed");
        }

        // ==========================
        // DISPATCH ORDER
        // ==========================
        [HttpPut("{orderId}/dispatch")]
        public IActionResult DispatchOrder(int orderId)
        {
            _orderService.DispatchOrder(orderId);
            return Ok("Order dispatched");
        }

        // ==========================
        // DELIVER ORDER
        // ==========================
        [HttpPut("{orderId}/deliver")]
        public IActionResult DeliverOrder(int orderId)
        {
            _orderService.DeliverOrder(orderId);
            return Ok("Order delivered");
        }

        // ==========================
        // CANCEL ORDER
        // ==========================
        [HttpPut("{orderId}/cancel")]
        public IActionResult CancelOrder(int orderId)
        {
            _orderService.CancelOrder(orderId);
            return Ok("Order cancelled");
        }

        // ==========================
        // RECENT ORDERS
        // ==========================
        [HttpGet("recent")]
        public IActionResult GetRecentOrders()
        {
            return Ok(_orderService.GetRecentOrders(10));
        }

        //Revert the order

        [HttpPut("{orderId}/revert")]
        public IActionResult RevertOrder(int orderId)
        {
            try
            {
                _orderService.RevertOrderStatus(orderId);
                return Ok("Order status reverted");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
