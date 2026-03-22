public class CreateWarehouseUserDto
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class UpdateWarehouseUserDto
{
    public string Name { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
}

public class UpdateUserStatusDto
{
    public bool IsActive { get; set; }
}
