public class ProductVariant
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }

    public string Size { get; set; } // Small, Medium, Large, X-Large

    public int Stock { get; set; }
    public decimal Price { get; set; }
}
