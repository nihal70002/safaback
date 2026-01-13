namespace PrivateECommerce.API.Models
{
    public class ProductVariant
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        // Navigation property: EF Core will handle this, 
        // null! tells the compiler it won't be null at runtime.
        public Product Product { get; set; } = null!;

        public required string Size { get; set; } // Small, Medium, Large, X-Large

        public int Stock { get; set; }
        public decimal Price { get; set; }
    }
}