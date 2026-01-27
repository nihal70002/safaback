using Microsoft.EntityFrameworkCore;
using PrivateECommerce.API.Data;
using PrivateECommerce.API.DTOs;
using PrivateECommerce.API.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace PrivateECommerce.API.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _cache;

        private const string ProductCacheVersionKey = "products_cache_version";

        public ProductService(AppDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public IEnumerable<ProductListDto> GetAllProducts()
        {
            return _context.Products
                .Include(p => p.Category)
.Include(p => p.Brand)     // ✅ ADD
.Include(p => p.Variants)

                .OrderByDescending(p => p.Id)
                .Select(p => new ProductListDto
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    BrandId = p.BrandId,                    // ✅ ADD
                    BrandName = p.Brand.BrandName,
                    ImageUrl = p.ImageUrl,
                    IsActive = p.IsActive,
                    Variants = p.Variants.Select(v => new ProductVariantListDto
                    {
                        VariantId = v.Id,
                        Size = v.Size,
                        Price = v.Price,
                        Stock = v.Stock
                    }).ToList()
                })
                .ToList();
        }


        // ===========================
        // CACHE VERSION HELPERS
        // ===========================
        private int GetCacheVersion()
        {
            var version = _cache.GetString(ProductCacheVersionKey);
            if (version == null)
            {
                _cache.SetString(ProductCacheVersionKey, "1");
                return 1;
            }
            return int.Parse(version);
        }

        private void IncrementCacheVersion()
        {
            var version = GetCacheVersion() + 1;
            _cache.SetString(ProductCacheVersionKey, version.ToString());
        }

        // ===========================
        // USER – LIST PRODUCTS (PAGED)
        // ===========================
        public PagedResponseDto<ProductListDto> GetProducts(int page, int pageSize)
        {
            int version = GetCacheVersion();
            string cacheKey = $"products_v{version}_page_{page}_{pageSize}";

            var cachedData = _cache.GetString(cacheKey);
            if (cachedData != null)
            {
                Console.WriteLine("PRODUCTS → REDIS CACHE HIT");
                return JsonSerializer.Deserialize<PagedResponseDto<ProductListDto>>(cachedData);
            }

            Console.WriteLine("PRODUCTS → DB HIT");

            var query = _context.Products
                .Where(p => p.IsActive)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Variants)
                .OrderByDescending(p => p.Id);

            var totalCount = query.Count();

            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductListDto
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    BrandId = p.BrandId,               // ✅ ADD
                    BrandName = p.Brand.BrandName,

                    ImageUrl = p.ImageUrl,
                    Variants = p.Variants.Select(v => new ProductVariantListDto
                    {
                        VariantId = v.Id,
                        Size = v.Size,
                        Price = v.Price,
                        Stock = v.Stock
                    }).ToList()
                })
                .ToList();

            var result = new PagedResponseDto<ProductListDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                HasMore = page * pageSize < totalCount
            };

            _cache.SetString(
                cacheKey,
                JsonSerializer.Serialize(result),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
                }
            );

            return result;
        }

        // ===========================
        // USER – PRODUCT DETAILS
        // ===========================
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

        // ===========================
        // ADMIN – CREATE PRODUCT
        // ===========================
        public void CreateProduct(AdminCreateProductDto dto)
        {
            if (!_context.Categories.Any(c => c.Id == dto.CategoryId))
                throw new Exception("Invalid Category");

            if (!_context.Brands.Any(b => b.BrandId == dto.BrandId))
                throw new Exception("Invalid Brand");

            var product = new Product
            {
                Name = dto.Name,
                CategoryId = dto.CategoryId,
                BrandId = dto.BrandId,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            _context.SaveChanges();

            if (dto.Variants != null)
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



        // ===========================
        // ADMIN – UPDATE PRODUCT
        // ===========================
        public void UpdateProduct(int productId, AdminUpdateProductDto dto)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
                throw new Exception("Product not found");

            // Optional validation (recommended)
            if (!_context.Brands.Any(b => b.BrandId == dto.BrandId))
                throw new Exception("Invalid Brand");

            if (!_context.Categories.Any(c => c.Id == dto.CategoryId))
                throw new Exception("Invalid Category");

            product.Name = dto.Name;
            product.CategoryId = dto.CategoryId;
            product.BrandId = dto.BrandId;     // ✅ MAIN FIX
            product.Description = dto.Description;
            product.ImageUrl = dto.ImageUrl;

            _context.SaveChanges();
            IncrementCacheVersion();
        }

        // ===========================
        // ADMIN – TOGGLE PRODUCT
        // ===========================
        public void ToggleProduct(int productId)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null) throw new Exception("Product not found");

            product.IsActive = !product.IsActive;
            _context.SaveChanges();

            IncrementCacheVersion();
        }

        // ===========================
        // ADMIN – DELETE PRODUCT (SOFT)
        // ===========================
        public void DeleteProduct(int productId)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null) throw new Exception("Product not found");

            product.IsActive = false;
            _context.SaveChanges();

            IncrementCacheVersion();
        }

        // ===========================
        // ADMIN – UPDATE VARIANT
        // ===========================
        public void UpdateProductVariant(int variantId, AdminUpdateProductVariantDto dto)
        {
            var variant = _context.ProductVariants.FirstOrDefault(v => v.Id == variantId);
            if (variant == null) throw new Exception("Variant not found");

            variant.Size = dto.Size;
            variant.Price = dto.Price;
            variant.Stock = dto.Stock;

            _context.SaveChanges();
            IncrementCacheVersion();
        }

        public void UpdateVariantStock(int variantId, int stock)
        {
            var variant = _context.ProductVariants.FirstOrDefault(v => v.Id == variantId);
            if (variant == null) throw new Exception("Variant not found");

            variant.Stock = stock;
            _context.SaveChanges();

            IncrementCacheVersion();
        }

        // ===========================
        // ADMIN – LOW STOCK
        // ===========================
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
    }
}
