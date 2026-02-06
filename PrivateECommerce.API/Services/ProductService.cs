using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using PrivateECommerce.API.Data;
using PrivateECommerce.API.DTOs;
using PrivateECommerce.API.Models;
using System.Text.Json;

namespace PrivateECommerce.API.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _cache;

        private const string ProductCacheVersionKey = "products_cache_version";
        private const int MAX_IMAGES = 5;

        public ProductService(AppDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // ===========================
        // INTERNAL / ADMIN – ALL PRODUCTS
        // ===========================
        public IEnumerable<ProductListDto> GetAllProducts()
        {
            return _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .OrderByDescending(p => p.Id)
                .Select(p => new ProductListDto
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    ProductCode = p.ProductCode,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    BrandId = p.BrandId,
                    BrandName = p.Brand.BrandName,

                    ImageUrls = p.Images
                        .OrderByDescending(i => i.IsPrimary)
                        .Select(i => i.ImageUrl)
                        .ToList(),

                    PrimaryImageUrl = p.Images
                        .OrderByDescending(i => i.IsPrimary)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault(),

                    IsActive = p.IsActive,

                    Variants = p.Variants.Select(v => new ProductVariantListDto
                    {
                        VariantId = v.Id,
                        Size = v.Size,
                        ProductCode = v.ProductCode,
                        Price = v.Price,
                        Stock = v.Stock
                    }).ToList()
                })
                .ToList();
        }

        // ===========================
        // CACHE HELPERS
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
            _cache.SetString(ProductCacheVersionKey, (GetCacheVersion() + 1).ToString());
        }

        // ===========================
        // USER – PAGED LIST
        // ===========================
        public PagedResponseDto<ProductListDto> GetProducts(int page, int pageSize)
        {
            int version = GetCacheVersion();
            string cacheKey = $"products_v{version}_page_{page}_{pageSize}";

            var cached = _cache.GetString(cacheKey);
            if (cached != null)
                return JsonSerializer.Deserialize<PagedResponseDto<ProductListDto>>(cached)!;

            var query = _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Variants)
                .Include(p => p.Images);

            var totalCount = query.Count();

            var items = query
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductListDto
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    BrandId = p.BrandId,
                    BrandName = p.Brand.BrandName,

                    PrimaryImageUrl = p.Images
                        .OrderByDescending(i => i.IsPrimary)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault(),

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

            _cache.SetString(cacheKey, JsonSerializer.Serialize(result),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
                });

            return result;
        }

        // ===========================
        // USER – PRODUCT DETAILS
        // ===========================
        public ProductDetailDto? GetProductById(int productId)
        {
            var product = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .FirstOrDefault(p => p.Id == productId && p.IsActive);

            if (product == null) return null;

            var images = product.Images
                .OrderByDescending(i => i.IsPrimary)
                .Select(i => i.ImageUrl)
                .ToList();

            return new ProductDetailDto
            {
                ProductId = product.Id,
                Name = product.Name,
                ProductCode = product.ProductCode,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                Description = product.Description,
                ImageUrls = images,
                PrimaryImageUrl = images.FirstOrDefault(),
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
            if (dto.ImageUrls.Count > MAX_IMAGES)
                throw new ValidationException($"Maximum {MAX_IMAGES} images allowed");

            var product = new Product
            {
                Name = dto.Name,
                CategoryId = dto.CategoryId,
                BrandId = dto.BrandId,
                Description = dto.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            _context.SaveChanges();

            for (int i = 0; i < dto.ImageUrls.Count; i++)
            {
                _context.ProductImages.Add(new ProductImage
                {
                    ProductId = product.Id,
                    ImageUrl = dto.ImageUrls[i],
                    IsPrimary = i == 0
                });
            }

            _context.SaveChanges();
            IncrementCacheVersion();
        }

        // ===========================
        // ADMIN – UPDATE PRODUCT
        // ===========================
        public void UpdateProduct(int productId, AdminUpdateProductDto dto)
        {
            if (dto.ImageUrls.Count > MAX_IMAGES)
                throw new ValidationException($"Maximum {MAX_IMAGES} images allowed");

            var product = _context.Products.FirstOrDefault(p => p.Id == productId)
                ?? throw new ValidationException("Product not found");

            product.Name = dto.Name;
            product.CategoryId = dto.CategoryId;
            product.BrandId = dto.BrandId;
            product.Description = dto.Description;

            _context.ProductImages.RemoveRange(
                _context.ProductImages.Where(i => i.ProductId == productId));

            for (int i = 0; i < dto.ImageUrls.Count; i++)
            {
                _context.ProductImages.Add(new ProductImage
                {
                    ProductId = productId,
                    ImageUrl = dto.ImageUrls[i],
                    IsPrimary = i == 0
                });
            }

            _context.SaveChanges();
            IncrementCacheVersion();
        }

        // ===========================
        // ADMIN – TOGGLE PRODUCT
        // ===========================
        public void ToggleProduct(int productId)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == productId)
                ?? throw new Exception("Product not found");

            product.IsActive = !product.IsActive;
            _context.SaveChanges();
            IncrementCacheVersion();
        }

        // ===========================
        // ADMIN – DELETE PRODUCT
        // ===========================
        public async Task DeleteProductAsync(int productId)
        {
            var product = await _context.Products
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                throw new Exception("Product not found");

            _context.ProductVariants.RemoveRange(product.Variants);
            _context.ProductImages.RemoveRange(product.Images);
            _context.Products.Remove(product);

            await _context.SaveChangesAsync();
            IncrementCacheVersion();
        }

        // ===========================
        // ADMIN – VARIANTS
        // ===========================
        public void AddProductVariant(int productId, AdminCreateProductVariantDto dto)
        {
            if (_context.ProductVariants.Any(v =>
                v.ProductId == productId &&
                v.Size.ToLower() == dto.Size.ToLower()))
                throw new ValidationException("Size already exists");

            _context.ProductVariants.Add(new ProductVariant
            {
                ProductId = productId,
                Size = dto.Size,
                ProductCode = dto.ProductCode,
                Price = dto.Price,
                Stock = dto.Stock
            });

            _context.SaveChanges();
            IncrementCacheVersion();
        }

        public void UpdateProductVariant(int variantId, AdminUpdateProductVariantDto dto)
        {
            var variant = _context.ProductVariants.FirstOrDefault(v => v.Id == variantId)
                ?? throw new ValidationException("Variant not found");

            variant.Size = dto.Size;
            variant.Price = dto.Price;
            variant.ProductCode = dto.ProductCode;
            variant.Stock = dto.Stock;

            _context.SaveChanges();
            IncrementCacheVersion();
        }

        public void UpdateVariantStock(int variantId, int stock)
        {
            var variant = _context.ProductVariants.FirstOrDefault(v => v.Id == variantId)
                ?? throw new Exception("Variant not found");

            variant.Stock = stock;
            _context.SaveChanges();
            IncrementCacheVersion();
        }

        // ===========================
        // ADMIN – BULK CREATE
        // ===========================
        public void BulkCreate(List<AdminBulkCreateProductDto> products)
        {
            foreach (var dto in products)
            {
                var product = new Product
                {
                    Name = dto.Name,
                    CategoryId = dto.CategoryId,
                    BrandId = dto.BrandId,
                    Description = dto.Description
                };

                _context.Products.Add(product);
                _context.SaveChanges();
            }

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
                })
                .ToList();
        }
    }
}
