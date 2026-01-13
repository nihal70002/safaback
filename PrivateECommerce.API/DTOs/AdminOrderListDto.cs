namespace PrivateECommerce.API.DTOs
{
    public class AdminOrderListDto
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string CompanyName { get; set; }
        public DateTime OrderDate { get; set; }
        public string? PhoneNumber { get; set; }

        public string Status { get; set; }
        public decimal TotalAmount { get; set; }


    }
}
