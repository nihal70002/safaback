using Microsoft.EntityFrameworkCore;
using ClientEcommerce.API.Data;
using ClientEcommerce.API.DTOs;
using ClientEcommerce.API.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace ClientEcommerce.API.Services
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
.Include(p => p.Images)


                .OrderByDescending(p => p.Id)
                .Select(p => new ProductListDto
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    ProductCode = p.ProductCode,
                    CategoryId = p.CategoryId,
                    Description = p.Description, // admin products description fix


                    CategoryName = p.Category.Name,
                    BrandId = p.BrandId,                    // ✅ ADD
                    BrandName = p.Brand.BrandName,
                    ImageUrls = p.Images
    .OrderByDescending(i => i.IsPrimary)
    .Select(i => i.ImageUrl)
    .ToList(),

                    PrimaryImageUrl = p.Images
    .Where(i => i.IsPrimary)
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
                .Include(p => p.Images)
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

                    Description = p.Description,

                    PrimaryImageUrl = p.Images
    .Where(i => i.IsPrimary)
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
                .Include(p => p.Images)

                .FirstOrDefault(p => p.Id == productId && p.IsActive);

            if (product == null) return null;

            return new ProductDetailDto
            {
                ProductId = product.Id,
                Name = product.Name,
                CategoryId = product.CategoryId,
                ProductCode = product.ProductCode,
                CategoryName = product.Category.Name,
                Description = product.Description,

                ImageUrls = product.Images
    .OrderByDescending(i => i.IsPrimary)
    .Select(i => i.ImageUrl)
    .ToList(),

                PrimaryImageUrl = product.Images
    .Where(i => i.IsPrimary)
    .Select(i => i.ImageUrl)
    .FirstOrDefault(),




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
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                // 1️⃣ Validations
                if (!_context.Categories.Any(c => c.Id == dto.CategoryId))
                    throw new ValidationException("Invalid category selected");

                if (!_context.Brands.Any(b => b.BrandId == dto.BrandId))
                    throw new ValidationException("Invalid Brand");

                if (dto.ImageUrls.Count > 5)
                    throw new ValidationException("Maximum 5 images allowed per product");

                var duplicateSizes = dto.Variants
                    .GroupBy(v => v.Size.Trim().ToLower())
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateSizes.Any())
                    throw new ValidationException(
                        $"Duplicate sizes not allowed: {string.Join(", ", duplicateSizes)}"
                    );

                foreach (var v in dto.Variants)
                {
                    var sku = v.ProductCode?.Trim();

                    if (string.IsNullOrWhiteSpace(sku))
                        throw new ValidationException("SKU / ProductCode is required");

                    bool skuExists = _context.ProductVariants.Any(pv =>
                        pv.ProductCode.ToLower() == sku.ToLower());

                    if (skuExists)
                        throw new ValidationException($"SKU already exists: {sku}");
                }

                // 2️⃣ Create Product
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

                // 3️⃣ Add Images
                for (int i = 0; i < dto.ImageUrls.Count; i++)
                {
                    _context.ProductImages.Add(new ProductImage
                    {
                        ProductId = product.Id,
                        ImageUrl = dto.ImageUrls[i],
                        IsPrimary = i == 0
                    });
                }

                // 4️⃣ Add Variants
                foreach (var v in dto.Variants)
                {
                    _context.ProductVariants.Add(new ProductVariant
                    {
                        ProductId = product.Id,
                        Size = v.Size,
                        ProductCode = v.ProductCode,
                        Price = v.Price,
                        Stock = v.Stock
                    });
                }

                _context.SaveChanges();

                // ✅ 1. Commit DB transaction
                transaction.Commit();

                // ✅ 2. Invalidate product cache
                IncrementCacheVersion();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }





        // ===========================
        // ADMIN – UPDATE PRODUCT
        // ===========================
        public void UpdateProduct(int productId, AdminUpdateProductDto dto)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
                throw new ValidationException("Product not found");

            if (!_context.Brands.Any(b => b.BrandId == dto.BrandId))
                throw new ValidationException("Invalid Brand");

            if (!_context.Categories.Any(c => c.Id == dto.CategoryId))
                throw new ValidationException("Invalid Category");

            product.Name = dto.Name;
            product.CategoryId = dto.CategoryId;

            product.BrandId = dto.BrandId;
            product.Description = dto.Description;
            _context.ProductImages.RemoveRange(
    _context.ProductImages.Where(i => i.ProductId == productId)
);

            if (dto.ImageUrls.Count > 5)
                throw new ValidationException("Maximum 5 images allowed");

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
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null) throw new Exception("Product not found");

            product.IsActive = !product.IsActive;
            _context.SaveChanges();

            IncrementCacheVersion();
        }

        // ===========================
        // ADMIN – DELETE PRODUCT (SOFT)
        // ===========================
        public async Task DeleteProductAsync(int productId)
        {
            var product = await _context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                throw new Exception("Product not found");

            // 🔥 HARD DELETE
            _context.ProductVariants.RemoveRange(product.Variants);
            _context.Products.Remove(product);

            await _context.SaveChangesAsync();
        }


        // ===========================
        // ADMIN – UPDATE VARIANT
        // ===========================
        public void UpdateProductVariant(int variantId, AdminUpdateProductVariantDto dto)
        {
            var variant = _context.ProductVariants.FirstOrDefault(v => v.Id == variantId);
            if (variant == null)
                throw new ValidationException("Variant not found");

            // ---------- SIZE VALIDATION ----------
            var size = dto.Size?.Trim();

            if (string.IsNullOrWhiteSpace(size))
                throw new ValidationException("Size cannot be empty");

            bool sizeExists = _context.ProductVariants.Any(v =>
                v.ProductId == variant.ProductId &&
                v.Size.ToLower() == size.ToLower() &&
                v.Id != variantId
            );

            if (sizeExists)
                throw new ValidationException($"Size '{size}' already exists for this product");

            // ---------- SKU VALIDATION ----------
            var sku = dto.ProductCode?.Trim();

            if (string.IsNullOrWhiteSpace(sku))
                throw new ValidationException("SKU cannot be empty");

            bool skuExists = _context.ProductVariants.Any(v =>
                v.ProductCode.ToLower() == sku.ToLower() &&
                v.Id != variantId
            );

            if (skuExists)
                throw new ValidationException($"SKU already exists: {sku}");

            // ---------- UPDATE ----------
            variant.Size = size;
            variant.Price = dto.Price;
            variant.ProductCode = sku;
            variant.Stock = dto.Stock;

            _context.SaveChanges();
            IncrementCacheVersion();
        }

        public void BulkCreate(List<AdminBulkCreateProductDto> products)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                foreach (var dto in products)
                {
                    var product = new Product
                    {
                        Name = dto.Name,
                        CategoryId = dto.CategoryId,
                        BrandId = dto.BrandId,
                        Description = dto.Description,
                        IsActive = true,                  // ✅ FIX 1
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Products.Add(product);
                    _context.SaveChanges(); // Needed to get product.Id

                    // ✅ Add product images (set primary)
                    if (dto.ImageUrls != null && dto.ImageUrls.Any())
                    {
                        bool isPrimary = true;

                        foreach (var imageUrl in dto.ImageUrls.Take(5))
                        {
                            _context.ProductImages.Add(new ProductImage
                            {
                                ProductId = product.Id,
                                ImageUrl = imageUrl,
                                IsPrimary = isPrimary       // ✅ FIX 2
                            });

                            isPrimary = false;
                        }
                    }

                    // ✅ Add variants (include stock)
                    foreach (var v in dto.Variants)
                    {
                        _context.ProductVariants.Add(new ProductVariant
                        {
                            ProductId = product.Id,
                            Size = v.Size,
                            ProductCode = v.ProductCode,
                            Price = v.Price,
                                          // ✅ FIX 3
                        });
                    }
                }

                _context.SaveChanges();
                transaction.Commit();

                IncrementCacheVersion();                  // ✅ FIX 4
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }







        public void UpdateVariantStock(int variantId, int stock)
        {
            var variant = _context.ProductVariants.FirstOrDefault(v => v.Id == variantId);
            if (variant == null) throw new Exception("Variant not found");

            variant.Stock = stock;
            _context.SaveChanges();

            IncrementCacheVersion();
        }
        public void AddProductVariant(int productId, AdminCreateProductVariantDto dto)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
                throw new ValidationException("Product not found");

            var size = dto.Size?.Trim();
            if (string.IsNullOrWhiteSpace(size))
                throw new ValidationException("Size cannot be empty");

            bool sizeExists = _context.ProductVariants.Any(v =>
                v.ProductId == productId &&
                v.Size.ToLower() == size.ToLower()
            );

            if (sizeExists)
                throw new ValidationException($"Size '{size}' already exists");

            var sku = dto.ProductCode?.Trim();
            if (string.IsNullOrWhiteSpace(sku))
                throw new ValidationException("SKU is required");

            bool skuExists = _context.ProductVariants.Any(v =>
                v.ProductCode.ToLower() == sku.ToLower());

            if (skuExists)
                throw new ValidationException($"SKU already exists: {sku}");

            _context.ProductVariants.Add(new ProductVariant
            {
                ProductId = productId,
                Size = size,
                ProductCode = sku,
                Price = dto.Price,
                Stock = dto.Stock
            });

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
