using System.ComponentModel.DataAnnotations.Schema;

namespace PrivateECommerce.API.Models
{
    [Table("ProductVariants")]
    public class ProductVariant
    {
        public int Id { get; set; }

        [Column("ProductId")] // ✅ MATCH DB EXACTLY
        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;

        public required string Size { get; set; }
        public int Stock { get; set; }
        public decimal Price { get; set; }
    }
}
