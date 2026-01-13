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

        public IEnumerable<ProductListDto> GetAllProducts()
        {
            return _context.Products
                .Where(p => p.IsActive)
                .Include(p => p.Category)
                .Include(p => p.Variants)
                .Select(p => new ProductListDto
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    ImageUrl = p.ImageUrl,
                    IsActive = p.IsActive,
                    Variants = p.Variants.Select(v => new ProductVariantListDto
                    {
                        VariantId = v.Id,
                        Size = v.Size,
                        Stock = v.Stock,
                        Price = v.Price
                    }).ToList()
                }).ToList();
        }

        public void CreateProduct(AdminCreateProductDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                CategoryId = dto.CategoryId,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            _context.SaveChanges();

            if (dto.Variants != null && dto.Variants.Any())
            {
                foreach (var v in dto.Variants)
                {
                    _context.ProductVariants.Add(new ProductVariant
                    {
                        ProductId = product.Id,
                        Size = v.Size,
                        Price = v.Price,
                        Stock = v.Stock
                    });
                }
                _context.SaveChanges();
            }
        }

        public void UpdateProduct(int productId, AdminUpdateProductDto dto)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null) throw new Exception("Product not found");

            product.Name = dto.Name;
            product.CategoryId = dto.CategoryId;
            product.Description = dto.Description;
            product.ImageUrl = dto.ImageUrl;
            

            _context.SaveChanges();
        }

        // Fixed to use AdminUpdateVariantDto and include Size
        // CHANGE THIS: AdminUpdateVariantDto -> AdminUpdateProductVariantDto
        public void UpdateProductVariant(int variantId, AdminUpdateProductVariantDto dto)
        {
            var variant = _context.ProductVariants.FirstOrDefault(v => v.Id == variantId);
            if (variant == null) throw new Exception("Variant not found");

            variant.Size = dto.Size;
            variant.Price = dto.Price;
            variant.Stock = dto.Stock;

            _context.SaveChanges();
        }

        // Added this to fix the Interface implementation error (CS0535)
        public void UpdateVariantStock(int variantId, int stock)
        {
            var variant = _context.ProductVariants.FirstOrDefault(v => v.Id == variantId);
            if (variant == null) throw new Exception("Variant not found");

            variant.Stock = stock;
            _context.SaveChanges();
        }

        public void ToggleProduct(int productId)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null) throw new Exception("Product not found");

            product.IsActive = !product.IsActive;
            _context.SaveChanges();
        }

        public void DeleteProduct(int productId)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null) throw new Exception("Product not found");

            // Soft delete to avoid Foreign Key errors seen in your screenshots
            product.IsActive = false;
            _context.SaveChanges();
        }

        public ProductDetailDto GetProductById(int productId)
        {
            var product = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Variants)
                .FirstOrDefault(p => p.Id == productId && p.IsActive);

            if (product == null) return null;

            return new ProductDetailDto
            {
                ProductId = product.Id,
                Name = product.Name,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
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
                }).ToList();
        }

        public PagedResponseDto<ProductListDto> GetProducts(int page, int pageSize)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Variants)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.Id);

            var totalCount = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(p => new ProductListDto
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    ImageUrl = p.ImageUrl,
                    Variants = p.Variants.Select(v => new ProductVariantListDto
                    {
                        VariantId = v.Id,
                        Size = v.Size,
                        Price = v.Price,
                        Stock = v.Stock
                    }).ToList()
                }).ToList();

            return new PagedResponseDto<ProductListDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                HasMore = page * pageSize < totalCount
            };
        }
    }
}