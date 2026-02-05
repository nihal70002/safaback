using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivateECommerce.API.Services;

[ApiController]
[Route("api/admin/warehouse-users")]
[Authorize(Roles = "Admin")]
public class AdminWarehouseUserController : ControllerBase
{
    private readonly IAdminWarehouseUserService _service;

    public AdminWarehouseUserController(IAdminWarehouseUserService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateWarehouseUserDto dto)
    {
        await _service.CreateAsync(dto);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> Update(int userId, UpdateWarehouseUserDto dto)
    {
        await _service.UpdateAsync(userId, dto);
        return Ok();
    }

    [HttpPatch("{userId}/status")]
    public async Task<IActionResult> UpdateStatus(int userId, UpdateUserStatusDto dto)
    {
        await _service.UpdateStatusAsync(userId, dto.IsActive);
        return Ok();
    }
}
