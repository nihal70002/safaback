using PrivateECommerce.API.DTOs;

public class ProductListDto
{
    public int ProductId { get; set; }
    public string Name { get; set; }

    public int CategoryId { get; set; }
    public string CategoryName { get; set; }

    public int BrandId { get; set; }          // ✅ ADD
    public string BrandName { get; set; }     // ✅ ADD

    public string ImageUrl { get; set; }
    public bool IsActive { get; set; }

    public List<ProductVariantListDto> Variants { get; set; }
}
