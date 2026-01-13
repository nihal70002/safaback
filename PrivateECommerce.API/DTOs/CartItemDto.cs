public class CartItemDto
{
    public int ProductVariantId { get; set; }
    public string ProductName { get; set; }
    public string Size { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string ImageUrl { get; set; }   // 🔴 ADD THIS
}
