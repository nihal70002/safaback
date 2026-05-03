using System.ComponentModel.DataAnnotations;

public class AddToCartDto
{
    [Range(1, int.MaxValue)]
    public int ProductVariantId { get; set; }

    [Range(1, 9999)]
    public int Quantity { get; set; }
}
