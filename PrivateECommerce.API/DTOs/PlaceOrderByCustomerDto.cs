using System.ComponentModel.DataAnnotations;

namespace PrivateECommerce.API.DTOs
{
    public class PlaceOrderByCustomerDto
    {
        [Required]
        [MinLength(1)]
        public List<PlaceOrderItemDto> Items { get; set; } = new();
    }

    public class PlaceOrderItemDto
    {
        [Range(1, int.MaxValue)]
        public int ProductVariantId { get; set; }

        [Range(1, 9999)]
        public int Quantity { get; set; }
    }
}
