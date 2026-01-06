public class AdminOrderDetailDto
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; }
    public string CompanyName { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; }
    public decimal TotalAmount { get; set; }
    public List<AdminOrderItemDto> Items { get; set; }
}

public class AdminOrderItemDto
{
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
