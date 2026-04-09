using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivateECommerce.API.Services;

[ApiController]
[Route("api/admin/sales-executives")]
[Authorize(Roles = "Admin")]
public class AdminSalesExecutivesController : ControllerBase
{
    private readonly IAdminSalesExecutiveService _service;

    public AdminSalesExecutivesController(IAdminSalesExecutiveService service)
    {
        _service = service;
    }

    // ============================
    // PERFORMANCE (ALREADY WORKING)
    // ============================
    [HttpGet("{salesExecutiveId}/performance")]
    public IActionResult GetSalesExecutivePerformance(int salesExecutiveId)
    {
        var result = _service.GetSalesExecutivePerformance(salesExecutiveId);
        return Ok(result);
    }

    // ============================
    // 🔥 ORDERS BY STATUS (MISSING API)
    // ============================
    // type = pending | accepted | rejected | completed | all
    [HttpGet("{salesExecutiveId}/orders")]
    public IActionResult GetOrdersByStatus(
        int salesExecutiveId,
        [FromQuery] string type)
    {
        var result = _service.GetOrdersByStatus(salesExecutiveId, type);
        return Ok(result);
    }
    [HttpGet("{salesExecutiveId}/customers")]
    public IActionResult GetCustomersForSalesExecutive(int salesExecutiveId)
    {
        var customers = _service.GetCustomersForSalesExecutive(salesExecutiveId);
        return Ok(customers);
    }


}
