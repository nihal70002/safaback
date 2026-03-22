using System.ComponentModel.DataAnnotations;

public class CreateSalesExecutiveDto
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string CompanyName { get; set; } = null!;

    [Required]
    public string Email { get; set; } = null!;

    [Required]
    public string PhoneNumber { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}
