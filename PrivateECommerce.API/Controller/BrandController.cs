using Microsoft.AspNetCore.Mvc;
using PrivateECommerce.API.DTOs;
using PrivateECommerce.API.Services;

namespace PrivateECommerce.API.Controllers
{
    [ApiController]
    [Route("api/brands")]
    public class BrandController : ControllerBase
    {
        private readonly IBrandService _service;

        public BrandController(IBrandService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult GetBrands()
        {
            return Ok(_service.GetBrands());
        }

        [HttpPost]
        public IActionResult AddBrand([FromBody] CreateBrandDto dto)
        {
            _service.CreateBrand(dto);
            return Ok("Brand created");
        }
    }
}
