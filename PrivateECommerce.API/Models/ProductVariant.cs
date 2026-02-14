using System.ComponentModel.DataAnnotations.Schema;

namespace PrivateECommerce.API.Models
{
    [Table("ProductVariants")]
    public class ProductVariant
    {
        public int Id { get; set; }

        [Column("ProductId")] // ✅ MATCH DB EXACTLY
        public int ProductId { get; set; }
        public string Class { get; set; } = null!;      // 1 / 2 / 3
        public string Style { get; set; } = null!;      // AD / AF / AG
        public string Material { get; set; } = null!;   // Classic / Cotton
        public string Color { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public int LowStockThreshold { get; set; } = 10;
        public string? ProductCode { get; set; } 


        public required string Size { get; set; }
        public int Stock { get; set; }
        public decimal Price { get; set; }
    }
}
