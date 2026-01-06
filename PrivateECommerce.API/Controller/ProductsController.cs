using Microsoft.AspNetCore.Mvc;
using PrivateECommerce.API.Services;

namespace PrivateECommerce.API.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // ===========================
        // LIST PRODUCTS (USER)
        // ===========================
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_productService.GetAllProducts());
        }

        // ===========================
        // PRODUCT DETAILS (USER)
        // ===========================
        [HttpGet("{productId}")]
        public IActionResult GetById(int productId)
        {
            var product = _productService.GetProductById(productId);

            if (product == null)
                return NotFound("Product not found");

            return Ok(product);
        }
    }
}
