using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivateECommerce.API.DTOs;
using PrivateECommerce.API.Services;

namespace PrivateECommerce.API.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoriesController(ICategoryService service)
        {
            _service = service;
        }

        // 🔓 Public (for users)
        [HttpGet]
        public IActionResult GetActiveCategories()
        {
            return Ok(_service.GetAll(admin: false));
        }

        // 🔐 Admin
        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult GetAllForAdmin()
        {
            return Ok(_service.GetAll(admin: true));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Create(CreateCategoryDto dto)
        {
            _service.Create(dto);
            return Ok(new { message = "Category created successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, UpdateCategoryDto dto)
        {
            _service.Update(id, dto);
            return Ok(new { message = "Category updated successfully" });
        }
    }
}
