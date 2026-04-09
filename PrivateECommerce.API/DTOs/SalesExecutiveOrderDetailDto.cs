public class SalesExecutiveOrderDetailDto
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }

    public CustomerDetailDto Customer { get; set; } = null!;
    public List<OrderItemDetailDto> Items { get; set; } = new();
}
