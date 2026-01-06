using Microsoft.EntityFrameworkCore;
using PrivateECommerce.API.Data;
using PrivateECommerce.API.DTOs;
using PrivateECommerce.API.Models;

namespace PrivateECommerce.API.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        // ===========================
        // ADMIN: CREATE PRODUCT
        // ===========================
        public void CreateProduct(AdminCreateProductDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Category = dto.Category,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                IsActive = true,
                Variants = dto.Variants.Select(v => new ProductVariant
                {
                    Size = v.Size,
                    Stock = v.Stock,
                    Price = v.Price
                }).ToList()
            };

            _context.Products.Add(product);
            _context.SaveChanges();
        }

        // ===========================
        // LIST PRODUCTS (ADMIN/USER)
        // ===========================
        public IEnumerable<ProductListDto> GetAllProducts()
        {
            return _context.Products
                .Where(p => p.IsActive)
                .Select(p => new ProductListDto
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    Category = p.Category,
                    ImageUrl = p.ImageUrl
                })
                .ToList();
        }

        // ===========================
        // PRODUCT DETAILS
        // ===========================
        public ProductDetailDto GetProductById(int productId)
        {
            var product = _context.Products
                .Include(p => p.Variants)
                .FirstOrDefault(p => p.Id == productId && p.IsActive);

            if (product == null)
                return null;

            return new ProductDetailDto
            {
                ProductId = product.Id,
                Name = product.Name,
                Category = product.Category,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                Sizes = product.Variants.Select(v => new ProductVariantDto
                {
                    VariantId = v.Id,
                    Size = v.Size,
                    Price = v.Price,
                    AvailableStock = v.Stock
                }).ToList()
            };
        }

        public void UpdateProductVariant(int variantId, AdminUpdateProductVariantDto dto)
        {
            var variant = _context.ProductVariants
                .FirstOrDefault(v => v.Id == variantId);

            if (variant == null)
                throw new Exception("Variant not found");

            variant.Price = dto.Price;
            variant.Stock = dto.Stock;

            _context.SaveChanges();
        }


        // ===========================
        // ADMIN: ENABLE / DISABLE
        // ===========================
        public void ToggleProduct(int productId)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
                throw new Exception("Product not found");

            product.IsActive = !product.IsActive;
            _context.SaveChanges();
        }

        // ===========================
        // ADMIN: UPDATE STOCK
        // ===========================
        public void UpdateVariantStock(int variantId, int stock)
        {
            var variant = _context.ProductVariants.FirstOrDefault(v => v.Id == variantId);
            if (variant == null)
                throw new Exception("Variant not found");

            variant.Stock = stock;
            _context.SaveChanges();
        }
        public void UpdateProduct(int productId, AdminUpdateProductDto dto)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);

            if (product == null)
                throw new Exception("Product not found");

            product.Name = dto.Name;
            product.Category = dto.Category;
            product.Description = dto.Description;
            product.ImageUrl = dto.ImageUrl;

            _context.SaveChanges();
        }

        public IEnumerable<LowStockVariantDto> GetLowStockVariants(int threshold)
        {
            return _context.ProductVariants
                .Include(v => v.Product)
                .Where(v => v.Stock <= threshold)
                .Select(v => new LowStockVariantDto
                {
                    VariantId = v.Id,
                    ProductName = v.Product.Name,
                    Size = v.Size,
                    Stock = v.Stock
                })
                .ToList();
        }


    }
}
