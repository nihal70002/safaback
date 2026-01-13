using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivateECommerce.API.DTOs;
using PrivateECommerce.API.Services;

namespace PrivateECommerce.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;

        public AdminController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("create-user")]
        public IActionResult CreateUser(CreateUserDto dto)
        {
            try
            {
                _userService.CreateCustomer(dto);
                return Ok("User created successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("users")]
        public IActionResult GetAllUsers()
        {
            var users = _userService.GetAllUsers();
            return Ok(users);
        }

        // GET: api/admin/users/{id}/details
        [HttpGet("users/{userId}/details")]
        public IActionResult GetUserDetails(int userId)
        {
            var details = _userService.GetUserDetails(userId);
            if (details == null) return NotFound("User not found");

            return Ok(details);
        }
        [HttpGet("users/{id}/purchase-insights")]
        public IActionResult GetPurchaseInsights(int id)
        {
            return Ok(_userService.GetUserPurchaseInsights(id));
        }


    }
}

