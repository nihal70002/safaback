public class CartItemDto
{
    public int ProductVariantId { get; set; }
    public string ProductName { get; set; }
    public string Size { get; set; }

    // 🔥 ADD THESE
    public string? Class { get; set; }
    public string? Style { get; set; }
    public string? Material { get; set; }
    public string? Color { get; set; }

    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string ImageUrl { get; set; }
}
