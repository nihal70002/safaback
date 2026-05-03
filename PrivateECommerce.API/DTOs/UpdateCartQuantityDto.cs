using System.ComponentModel.DataAnnotations;

namespace PrivateECommerce.API.DTOs
{
    public class UpdateCartQuantityDto
    {
        [Range(1, int.MaxValue)]
        public int ProductVariantId { get; set; }

        [Range(0, 9999)]
        public int Quantity { get; set; }
    }
}
