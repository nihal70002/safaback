public class SalesExecutivePerformanceDto
{
    public int SalesExecutiveId { get; set; }
    public string Name { get; set; } = string.Empty;

    public int TotalCustomers { get; set; }
    public int ActiveCustomers { get; set; }

    public int TotalOrders { get; set; }
    public int AcceptedOrders { get; set; }
    public int RejectedOrders { get; set; }
    public int PendingOrders { get; set; }

    public decimal TotalOrderValue { get; set; }

    public DateTime? LastOrderDate { get; set; }
}
