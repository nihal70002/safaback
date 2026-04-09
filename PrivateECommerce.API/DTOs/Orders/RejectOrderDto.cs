using System.ComponentModel.DataAnnotations;

namespace PrivateECommerce.API.DTOs.Orders
{
    public class RejectOrderDto
    {
        [Required]
        public string Reason { get; set; } = null!;
    }
}
