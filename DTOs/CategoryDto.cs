namespace ClientEcommerce.API.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public bool IsActive { get; set; }
    }
}