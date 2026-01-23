using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivateECommerce.API.DTOs;
using PrivateECommerce.API.DTOs.Admin;
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

        // ==========================
        // ADMIN: CREATE CUSTOMER
        // ==========================
        [HttpPost("customers")]
        public IActionResult CreateCustomer(
    [FromBody] CreateCustomerByAdminDto dto)
        {
            _userService.CreateCustomerByAdmin(dto);
            return Ok(new { message = "Customer created successfully" });
        }
        // ============================
        // ADMIN: LIST SALES EXECUTIVES (HELPER)
        // ============================
        [HttpGet("sales-executives")]
        public IActionResult GetSalesExecutives([FromQuery] string? search)
        {
            var result = _userService.GetSalesExecutives(search);
            return Ok(result);
        }


        // ==========================
        // ADMIN: CREATE SALES EXECUTIVE
        // ==========================
        [HttpPost("create-sales-executive")]
        public IActionResult CreateSalesExecutive(CreateSalesExecutiveDto dto)
        {
            try
            {
                var user = _userService.CreateSalesExecutive(dto);
                return Ok(new
                {
                    message = "Sales executive created successfully",
                    userId = user.Id
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }


        // ==========================
        // ADMIN: GET USERS
        // ==========================
        [HttpGet("users")]
        public IActionResult GetAllUsers()
        {
            return Ok(_userService.GetAllUsers());
        }

        // ==========================
        // ADMIN: USER DETAILS
        // ==========================
        [HttpGet("users/{userId}/details")]
        public IActionResult GetUserDetails(int userId)
        {
            var details = _userService.GetUserDetails(userId);
            if (details == null)
                return NotFound(new { message = "User not found" });

            return Ok(details);
        }

        // ==========================
        // ADMIN: PURCHASE INSIGHTS
        // ==========================
        [HttpGet("users/{userId}/purchase-insights")]
        public IActionResult GetPurchaseInsights(int userId)
        {
            return Ok(_userService.GetUserPurchaseInsights(userId));
        }

        // ==========================
        // ADMIN: GET SALES EXECUTIVES
        // ==========================
        [HttpGet("sales-executives/summary")]
        public IActionResult GetSalesExecutivesSummary()
        {
            var data = _userService.GetAllSalesExecutivesForAdmin();
            return Ok(data);
        }

        //[HttpGet("sales-executives/{id}/performance")]
        //public IActionResult GetSalesExecutivePerformance(int id)
        //{
        //    var result = _userService.GetSalesExecutivePerformance(id);
        //    return Ok(result);
        //}
        [HttpPut("{salesExecutiveId}")]
        public IActionResult UpdateSalesExecutive(
          int salesExecutiveId,
          UpdateSalesExecutiveDto dto)
        {
            _userService.UpdateSalesExecutive(salesExecutiveId, dto);
            return Ok("Sales Executive updated successfully");
        }

        // ============================
        // DELETE SALES EXECUTIVE
        // ============================
        [HttpDelete("{salesExecutiveId}")]
        public IActionResult DeleteSalesExecutive(int salesExecutiveId)
        {
            _userService.DeleteSalesExecutive(salesExecutiveId);
            return Ok("Sales Executive deleted successfully");
        }
        // ============================
        // ADMIN: ASSIGN SALES EXECUTIVE TO CUSTOMER
        // ============================
        [HttpPut("customers/{customerId}/assign-sales-executive")]
        public IActionResult AssignSalesExecutiveToCustomer(
            int customerId,
            AssignSalesExecutiveDto dto)
        {
            _userService.AssignSalesExecutiveToCustomer(customerId, dto.SalesExecutiveId);
            return Ok(new { message = "Sales Executive assigned successfully" });
        }


    }
}
