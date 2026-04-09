namespace PrivateECommerce.API.DTOs.Admin
{
    public class AdminCustomerDto
    {
        public int UserId { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string CompanyName { get; set; }
        public string PhoneNumber { get; set; }

        public int? SalesExecutiveId { get; set; }
        public string? SalesExecutiveName { get; set; }
    }
}
