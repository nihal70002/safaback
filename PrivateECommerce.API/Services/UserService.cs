using PrivateECommerce.API.Data;
using PrivateECommerce.API.DTOs;
using PrivateECommerce.API.Models;

namespace PrivateECommerce.API.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public void CreateCustomer(CreateUserDto dto)
        {
            if (_context.Users.Any(u => u.Email == dto.Email))
                throw new Exception("User already exists");

            var user = new User
            {
                Name = dto.Name,
                CompanyName = dto.CompanyName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Role = "Customer",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IsActive = true
            };

            _context.Users.Add(user);
            _context.SaveChanges();
        }
    }
}
