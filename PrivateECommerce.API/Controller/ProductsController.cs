using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Roles = "Admin")]
        public IActionResult BulkCreateProducts([FromBody] List<AdminBulkCreateProductDto> products)
        {
            _productService.BulkCreate(products);
            return Ok(ApiResponse.Ok("Bulk products created successfully"));
        }

        [HttpGet]
        public IActionResult GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12,
            [FromQuery] List<int>? categoryIds = null,
            [FromQuery] int? brandId = null,
            [FromQuery] string? search = null)
        {
            var result = _productService.GetProducts(
                page,
                pageSize,
                categoryIds,
                brandId,
                search);

            return Ok(result);
        }

        [HttpDelete("{productId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            await _productService.DeleteProductAsync(productId);
            return Ok(ApiResponse.Ok("Product deleted successfully"));
        }

        [HttpPut("{productId:int}")]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateProduct(int productId, [FromBody] AdminUpdateProductDto dto)
        {
            _productService.UpdateProduct(productId, dto);
            return Ok(ApiResponse.Ok("Product updated successfully"));
        }

        [HttpGet("search-suggestions")]
        public IActionResult SearchSuggestions([FromQuery] string query)
        {
            var results = _productService.SearchSuggestions(query);
            return Ok(results);
        }

        [HttpGet("{productId:int}")]
        public IActionResult GetById(int productId)
        {
            var product = _productService.GetProductById(productId);

            if (product == null)
                return NotFound(ApiResponse.Fail("Product not found"));

            return Ok(product);
        }
    }
}
