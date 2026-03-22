using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivateECommerce.API.Services;
using System.Security.Claims;

namespace PrivateECommerce.API.Controllers
{
    [ApiController]
    [Route("api/warehouse")]
    [Authorize(Roles = "Warehouse,Admin")]
    public class WarehouseController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;

        public WarehouseController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        // =============================
        // 📊 DASHBOARD
        // =============================
        [HttpGet("orders")]
        public IActionResult GetAllOrders()
        {
            return Ok(_warehouseService.GetAllOrders());
        }

        // =============================
        // 📦 WAREHOUSE ACTIONS
        // =============================
        [HttpPost("orders/{orderId}/dispatch")]
        public IActionResult DispatchOrder(int orderId)
        {
            _warehouseService.DispatchOrder(orderId);
            return Ok(new { message = "Order dispatched" });
        }

        [HttpPost("orders/{orderId}/deliver")]
        public IActionResult DeliverOrder(int orderId)
        {
            _warehouseService.DeliverOrder(orderId);
            return Ok(new { message = "Order delivered" });
        }

        [HttpPost("orders/{orderId}/approve")]
        public IActionResult ApproveOrder(int orderId)
        {
            int warehouseUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            _warehouseService.ApproveOrder(orderId, warehouseUserId);
            return Ok("Order approved by warehouse");
        }

        [HttpPost("orders/{orderId}/reject")]
        public IActionResult RejectOrder(
            int orderId,
            [FromBody] string reason
        )
        {
            if (string.IsNullOrWhiteSpace(reason))
                return BadRequest("Rejection reason is required");

            int warehouseUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            _warehouseService.RejectOrder(orderId, warehouseUserId, reason);
            return Ok("Order rejected by warehouse");
        }

        // =============================
        // 📊 DASHBOARD – TODAY
        // =============================
        [HttpGet("dashboard/summary")]
        public IActionResult GetTodaySummary()
        {
            return Ok(_warehouseService.GetTodayOrderSummary());
        }

        [HttpGet("today-orders")]
        public IActionResult GetTodayOrders()
        {
            return Ok(_warehouseService.GetTodayOrders());
        }

        // =============================
        // 📄 ORDER DETAILS
        // =============================
        [HttpGet("orders/{orderId}")]
        public IActionResult GetOrderDetails(int orderId)
        {
            return Ok(_warehouseService.GetOrderDetails(orderId));
        }

        // =============================
        // 📦 INVENTORY
        // =============================
        [HttpGet("inventory")]
        public IActionResult GetInventory()
        {
            return Ok(_warehouseService.GetInventory());
        }

        [HttpGet("inventory/low-stock")]
        public IActionResult GetLowStock([FromQuery] int threshold = 10)
        {
            return Ok(_warehouseService.GetLowStockProducts(threshold));
        }

        [HttpGet("inventory/low-stock-alerts")]
        public IActionResult LowStockAlerts()
        {
            return Ok(_warehouseService.GetLowStockAlerts());
        }

        [HttpGet("inventory/stock-movements")]
        public IActionResult StockMovements()
        {
            return Ok(_warehouseService.GetStockMovements());
        }

        // =============================
        // 👤 SALES EXECUTIVES
        // =============================
        [HttpGet("sales-executives")]
        public IActionResult GetSalesExecutives()
        {
            return Ok(_warehouseService.GetSalesExecutivesWithOrders());
        }

        [HttpGet("sales-executives/{salesExecutiveId}/orders")]
        public IActionResult GetOrdersBySalesExecutive(
            int salesExecutiveId,
            [FromQuery] string? status,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate
        )
        {
            return Ok(
                _warehouseService.GetOrdersBySalesExecutive(
                    salesExecutiveId,
                    status,
                    fromDate,
                    toDate
                )
            );
        }
        

        // =============================
        // ⏳ PENDING ORDERS
        // =============================
        [HttpGet("orders/pending")]
        public IActionResult GetPendingOrders()
        {
            return Ok(_warehouseService.GetPendingOrders());
        }
    }
}
