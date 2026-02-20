using Microsoft.AspNetCore.Mvc;
using PrivateECommerce.API.DTOs;
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
        [HttpPost("bulk")]
        public IActionResult BulkCreateProducts(
    [FromBody] List<AdminBulkCreateProductDto> products)
        {
            _productService.BulkCreate(products);
            return Ok(new { message = "Bulk products created successfully" });
        }

        // ===========================
        // LIST PRODUCTS (USER) - PAGINATED
        // GET: api/products?page=1&pageSize=12
        // ===========================

        [HttpGet]
        public IActionResult GetAll(
      [FromQuery] int page = 1,
      [FromQuery] int pageSize = 12,
      [FromQuery] int? categoryId = null,
      [FromQuery] int? brandId = null,
      [FromQuery] string? search = null)
        {
            var result = _productService.GetProducts(
                page,
                pageSize,
                categoryId,
                brandId,
                search);

            return Ok(result);
        }








      
        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            await _productService.DeleteProductAsync(productId);
            return Ok();
        }
        [HttpPut("{productId:int}")]
        public IActionResult UpdateProduct(int productId, [FromBody] AdminUpdateProductDto dto)
        {
            try
            {
                _productService.UpdateProduct(productId, dto);
                return Ok(new { message = "Product updated successfully" });
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // This will help you see the real error in your logs
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
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
