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
                    Description = p.Description,

                    CategoryName = p.Category.Name,
                    BrandId = p.BrandId,
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

                    // 🔥 UPDATED VARIANT MAPPING
                    Variants = p.Variants.Select(v => new ProductVariantListDto
                    {
                        VariantId = v.Id,
                        Size = v.Size,

                        // ✅ ADD THESE
                        Class = v.Class,
                        Style = v.Style,
                        Material = v.Material,
                        Color = v.Color,

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





        public PagedResponseDto<ProductListDto> GetProducts(
    int page,
    int pageSize,
    List<int>? categoryIds,
    int? brandId,
    string? search)
        {
            int version = GetCacheVersion();

            // 🔥 SAFE CATEGORY CACHE KEY
            var categoryKey = categoryIds != null && categoryIds.Any()
                ? string.Join("-", categoryIds.OrderBy(x => x))
                : "none";

            string cacheKey =
                $"products_v{version}_page_{page}_{pageSize}_cats_{categoryKey}_brand_{brandId}_search_{search}";

            var cachedData = _cache.GetString(cacheKey);
            if (cachedData != null)
            {
                Console.WriteLine("PRODUCTS → REDIS CACHE HIT");
                return JsonSerializer.Deserialize<PagedResponseDto<ProductListDto>>(cachedData);
            }

            Console.WriteLine("PRODUCTS → DB HIT");

            var query = _context.Products
    .Include(p => p.Brand)   // move include BEFORE filtering
    .Where(p => p.IsActive);

            // 🔥 MULTI CATEGORY FILTER
            if (categoryIds != null && categoryIds.Any())
            {
                query = query.Where(p => categoryIds.Contains(p.CategoryId));
            }

            if (brandId.HasValue)
                query = query.Where(p => p.BrandId == brandId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(p =>
                    EF.Functions.ILike(p.Name, $"%{search}%") ||
                    EF.Functions.ILike(p.Description, $"%{search}%") ||
                    EF.Functions.ILike(p.Brand.BrandName, $"%{search}%")
                );
            }

            query = query
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
                    BrandId = p.BrandId,
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
                });

            return result;
        }


        public List<ProductSearchSuggestionDto> SearchSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<ProductSearchSuggestionDto>();

            query = query.Trim();

            return _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Where(p => p.IsActive &&
                    EF.Functions.ILike(p.Name, $"%{query}%"))
                .OrderByDescending(p => p.Id)
                .Take(5)
                .Select(p => new ProductSearchSuggestionDto
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    BrandName = p.Brand.BrandName,
                    PrimaryImageUrl = p.Images
                        .Where(i => i.IsPrimary)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault(),
                    StartingPrice = p.Variants
                        .OrderBy(v => v.Price)
                        .Select(v => v.Price)
                        .FirstOrDefault()
                })
                .ToList();
        }


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

                // 🔥 UPDATED VARIANT MAPPING
                Sizes = product.Variants.Select(v => new ProductVariantDto
                {
                    VariantId = v.Id,
                    Size = v.Size,

                    Class = v.Class,
                    Style = v.Style,
                    Material = v.Material,
                    Color = v.Color,

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

                var duplicateCombinations = dto.Variants
    .GroupBy(v => new
    {
        Class = v.Class.Trim().ToLower(),
        Style = v.Style.Trim().ToLower(),
        Material = v.Material.Trim().ToLower(),
        Color = v.Color.Trim().ToLower(),
        Size = v.Size.Trim().ToLower()
    })
    .Where(g => g.Count() > 1)
    .ToList();

                if (duplicateCombinations.Any())
                    throw new ValidationException("Duplicate variant combination detected.");


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
                        Class = v.Class?.Trim(),
                        Style = v.Style?.Trim(),
                        Material = v.Material?.Trim(),
                        Color = v.Color?.Trim(),
                        Size = v.Size?.Trim(),
                        ProductCode = v.ProductCode?.Trim(),
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
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var product = _context.Products.FirstOrDefault(p => p.Id == productId);
                if (product == null) throw new ValidationException("Product not found");

                // 1. Basic Updates
                product.Name = dto.Name;
                product.CategoryId = dto.CategoryId;
                product.BrandId = dto.BrandId;
                product.Description = dto.Description;

                // 2. Clear and Replace Images
                var existingImages = _context.ProductImages.Where(i => i.ProductId == productId);
                _context.ProductImages.RemoveRange(existingImages);

                if (dto.ImageUrls.Count > 5)
                    throw new ValidationException("Maximum 5 images allowed");

                for (int i = 0; i < dto.ImageUrls.Count; i++)
                {
                    _context.ProductImages.Add(new ProductImage
                    {
                        ProductId = productId,
                        ImageUrl = dto.ImageUrls[i],
                        IsPrimary = (i == 0)
                    });
                }

                // 3. Save DB Changes
                _context.SaveChanges();

                // 4. Update Cache (If this fails, DB will rollback)
                IncrementCacheVersion();

                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
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
            var variant = _context.ProductVariants
                .FirstOrDefault(v => v.Id == variantId);

            if (variant == null)
                throw new ValidationException("Variant not found");

            // ---------- CLEAN VALUES ----------
            var classValue = dto.Class?.Trim();
            var style = dto.Style?.Trim();
            var material = dto.Material?.Trim();
            var color = dto.Color?.Trim();
            var size = dto.Size?.Trim();
            var sku = dto.ProductCode?.Trim();

            // ---------- REQUIRED FIELDS ----------
            if (string.IsNullOrWhiteSpace(size))
                throw new ValidationException("Size is required");

            if (string.IsNullOrWhiteSpace(sku))
                throw new ValidationException("SKU cannot be empty");

            // ---------- COMBINATION VALIDATION ----------
            bool combinationExists = _context.ProductVariants.Any(v =>
                v.ProductId == variant.ProductId &&
                (v.Class ?? "").ToLower() == (classValue ?? "").ToLower() &&
                (v.Style ?? "").ToLower() == (style ?? "").ToLower() &&
                (v.Material ?? "").ToLower() == (material ?? "").ToLower() &&
                (v.Color ?? "").ToLower() == (color ?? "").ToLower() &&
                (v.Size ?? "").ToLower() == size.ToLower() &&
                v.Id != variantId
            );

            if (combinationExists)
                throw new ValidationException("Variant combination already exists.");

            // ---------- SKU VALIDATION ----------
            bool skuExists = _context.ProductVariants.Any(v =>
                (v.ProductCode ?? "").ToLower() == sku.ToLower() &&
                v.Id != variantId
            );

            if (skuExists)
                throw new ValidationException($"SKU already exists: {sku}");

            // ---------- UPDATE ----------
            variant.Class = classValue;
            variant.Style = style;
            variant.Material = material;
            variant.Color = color;
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
                    // ---------- BASIC VALIDATION ----------
                    if (!_context.Categories.Any(c => c.Id == dto.CategoryId))
                        throw new ValidationException($"Invalid category for product: {dto.Name}");

                    if (!_context.Brands.Any(b => b.BrandId == dto.BrandId))
                        throw new ValidationException($"Invalid brand for product: {dto.Name}");

                    if (dto.ImageUrls?.Count > 5)
                        throw new ValidationException($"Maximum 5 images allowed for product: {dto.Name}");

                    // ---------- VARIANT COMBINATION VALIDATION ----------
                    var duplicateCombinations = dto.Variants
                        .GroupBy(v => new
                        {
                            Class = v.Class.Trim().ToLower(),
                            Style = v.Style.Trim().ToLower(),
                            Material = v.Material.Trim().ToLower(),
                            Color = v.Color.Trim().ToLower(),
                            Size = v.Size.Trim().ToLower()
                        })
                        .Where(g => g.Count() > 1)
                        .ToList();

                    if (duplicateCombinations.Any())
                        throw new ValidationException($"Duplicate variant combinations found in product: {dto.Name}");

                    // ---------- CREATE PRODUCT ----------
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
                    _context.SaveChanges(); // get product.Id

                    // ---------- ADD IMAGES ----------
                    if (dto.ImageUrls != null && dto.ImageUrls.Any())
                    {
                        bool isPrimary = true;

                        foreach (var imageUrl in dto.ImageUrls.Take(5))
                        {
                            _context.ProductImages.Add(new ProductImage
                            {
                                ProductId = product.Id,
                                ImageUrl = imageUrl,
                                IsPrimary = isPrimary
                            });

                            isPrimary = false;
                        }
                    }

                    // ---------- ADD VARIANTS ----------
                    foreach (var v in dto.Variants)
                    {
                        var sku = v.ProductCode?.Trim();
                        if (string.IsNullOrWhiteSpace(sku))
                            throw new ValidationException($"SKU is required in product: {dto.Name}");

                        bool skuExists = _context.ProductVariants
                            .Any(pv => pv.ProductCode.ToLower() == sku.ToLower());

                        if (skuExists)
                            throw new ValidationException($"Duplicate SKU detected: {sku}");

                        _context.ProductVariants.Add(new ProductVariant
                        {
                            ProductId = product.Id,
                            Class = v.Class.Trim(),
                            Style = v.Style.Trim(),
                            Material = v.Material.Trim(),
                            Color = v.Color.Trim(),
                            Size = v.Size.Trim(),
                            ProductCode = sku,
                            Price = v.Price,
                            
                            LowStockThreshold = 10
                        });
                    }
                }

                _context.SaveChanges();
                transaction.Commit();

                IncrementCacheVersion();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }







        public void UpdateVariantStock(int variantId, int stock)
        {
            if (stock < 0)
                throw new ValidationException("Stock cannot be negative");

            var variant = _context.ProductVariants
                .FirstOrDefault(v => v.Id == variantId);

            if (variant == null)
                throw new ValidationException("Variant not found");

            variant.Stock = stock;

            _context.SaveChanges();
            IncrementCacheVersion();
        }

        public void AddProductVariant(int productId, AdminCreateProductVariantDto dto)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
                throw new ValidationException("Product not found");

            // ---------- REQUIRED FIELD VALIDATION ----------
            var classValue = dto.Class?.Trim();
            var style = dto.Style?.Trim();
            var material = dto.Material?.Trim();
            var color = dto.Color?.Trim();
            var size = dto.Size?.Trim();

            

            if (string.IsNullOrWhiteSpace(size))
                throw new ValidationException("Size is required");

            if (dto.Stock < 0)
                throw new ValidationException("Stock cannot be negative");

            // ---------- COMBINATION VALIDATION ----------
            bool combinationExists = _context.ProductVariants.Any(v =>
                v.ProductId == productId &&
                v.Class.ToLower() == classValue.ToLower() &&
                v.Style.ToLower() == style.ToLower() &&
                v.Material.ToLower() == material.ToLower() &&
                v.Color.ToLower() == color.ToLower() &&
                v.Size.ToLower() == size.ToLower()
            );

            if (combinationExists)
                throw new ValidationException("This variant combination already exists");

            // ---------- SKU VALIDATION ----------
            var sku = dto.ProductCode?.Trim();

            if (string.IsNullOrWhiteSpace(sku))
                throw new ValidationException("SKU is required");

            bool skuExists = _context.ProductVariants.Any(v =>
                v.ProductCode.ToLower() == sku.ToLower());

            if (skuExists)
                throw new ValidationException($"SKU already exists: {sku}");

            // ---------- INSERT ----------
            _context.ProductVariants.Add(new ProductVariant
            {
                ProductId = productId,
                Class = classValue,
                Style = style,
                Material = material,
                Color = color,
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
