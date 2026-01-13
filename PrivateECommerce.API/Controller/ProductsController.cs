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
        // LIST PRODUCTS (USER) - PAGINATED
        // GET: api/products?page=1&pageSize=12
        // ===========================
        [HttpGet]
        public IActionResult GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12)
        {
            var result = _productService.GetProducts(page, pageSize);
            return Ok(result);
        }

        // ===========================
        // PRODUCT DETAILS (USER)
        // GET: api/products/5
        // ===========================
        [HttpGet("{productId:int}")]
        public IActionResult GetById(int productId)
        {
            var product = _productService.GetProductById(productId);

            if (product == null)
                return NotFound("Product not found");

            return Ok(product);
        }
    }
}
