public class UserProfileDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string CompanyName { get; set; }   // 👈 add
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}
