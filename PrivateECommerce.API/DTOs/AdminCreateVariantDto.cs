using System.ComponentModel.DataAnnotations;

public class AdminCreateVariantDto
{
    [Required]
    [StringLength(80)]
    public string? Size { get; set; }

    [Required]
    [StringLength(80)]
    public string Class { get; set; } = null!;

    [Required]
    [StringLength(80)]
    public string Style { get; set; } = null!;

    [Required]
    [StringLength(80)]
    public string Material { get; set; } = null!;

    [Required]
    [StringLength(80)]
    public string Color { get; set; } = null!;

    [Required]
    [StringLength(80)]
    public string? ProductCode { get; set; }

    [Range(0.01, 999999999)]
    public decimal Price { get; set; }
}
