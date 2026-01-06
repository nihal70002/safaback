using PrivateECommerce.API.DTOs;

public interface IProductService
{
    // CREATE
    void CreateProduct(AdminCreateProductDto dto);

    // READ
    IEnumerable<ProductListDto> GetAllProducts();
    ProductDetailDto GetProductById(int productId);

    // UPDATE
    void UpdateProduct(int productId, AdminUpdateProductDto dto);
    void UpdateProductVariant(int variantId, AdminUpdateProductVariantDto dto);

    // STATUS / STOCK
    void ToggleProduct(int productId);
    void UpdateVariantStock(int variantId, int stock);

    IEnumerable<LowStockVariantDto> GetLowStockVariants(int threshold);

}
