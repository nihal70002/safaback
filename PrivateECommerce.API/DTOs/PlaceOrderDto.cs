namespace PrivateECommerce.API.DTOs
{
    public class PlaceOrderDto
    {
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public int ProductVariantId { get; set; }
        public int Quantity { get; set; }
    }
}
