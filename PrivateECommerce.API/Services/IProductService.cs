using PrivateECommerce.API.DTOs;

namespace PrivateECommerce.API.Services
{
    public interface IProductService
    {
        IEnumerable<ProductListDto> GetAllProducts();
        ProductDetailDto GetProductById(int productId);
        void CreateProduct(AdminCreateProductDto dto); // Matches AdminProductsController
        void UpdateProduct(int productId, AdminUpdateProductDto dto);
        void UpdateProductVariant(int variantId, AdminUpdateProductVariantDto dto);
        void UpdateVariantStock(int variantId, int stock);
        void ToggleProduct(int productId);
        void DeleteProduct(int productId);
        IEnumerable<LowStockVariantDto> GetLowStockVariants(int threshold);
        PagedResponseDto<ProductListDto> GetProducts(int page, int pageSize);
    }
}