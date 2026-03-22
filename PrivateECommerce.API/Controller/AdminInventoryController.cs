using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivateECommerce.API.Services;

namespace PrivateECommerce.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/inventory")]
    [Authorize(Roles = "Admin")]
    public class AdminInventoryController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;

        public AdminInventoryController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        [HttpGet]
        public IActionResult Inventory()
        {
            return Ok(_warehouseService.GetInventory());
        }

        [HttpGet("low-stock")]
        public IActionResult LowStock()
        {
            return Ok(_warehouseService.GetLowStockProducts(10));
        }

        [HttpGet("stock-movements")]
        public IActionResult StockMovements()
        {
            return Ok(_warehouseService.GetStockMovements());
        }
    }
}
