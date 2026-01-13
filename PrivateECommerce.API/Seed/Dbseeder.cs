using PrivateECommerce.API.Data;
using PrivateECommerce.API.Models;

namespace PrivateECommerce.API.Seed
{
    public static class DbSeeder
    {
        public static void SeedAdmin(AppDbContext context)
        {
            if (!context.Users.Any())
            {
                var admin = new User
                {
                    Name = "Super Admin",
                    Email = "admin@company.com",
                    CompanyName = "Private E-Commerce",
                    PhoneNumber = "7591907000",
                    Role = "Admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    IsActive = true
                };

                context.Users.Add(admin);
                context.SaveChanges();
            }
        }
    }
}
