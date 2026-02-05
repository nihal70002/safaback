using Microsoft.EntityFrameworkCore;
using PrivateECommerce.API.Data;
using PrivateECommerce.API.DTOs;
using PrivateECommerce.API.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;


namespace PrivateECommerce.API.Services
{
    public class AdminWarehouseUserService : IAdminWarehouseUserService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AdminWarehouseUserService(AppDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // =========================
        // CREATE
        // =========================
        public async Task CreateAsync(CreateWarehouseUserDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                throw new Exception("Email already exists");

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Role = "Warehouse",
                IsActive = true
            };

            user.PasswordHash =
                _passwordHasher.HashPassword(user, dto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        // =========================
        // GET ALL
        // =========================
        public async Task<IEnumerable<object>> GetAllAsync()
        {
            return await _context.Users
                .Where(u => u.Role == "Warehouse")
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.PhoneNumber,
                    u.IsActive
                })
                .ToListAsync();
        }

        // =========================
        // UPDATE
        // =========================
        public async Task UpdateAsync(int userId, UpdateWarehouseUserDto dto)
        {
            var user = await _context.Users.FindAsync(userId)
                ?? throw new Exception("User not found");

            user.Name = dto.Name;
            user.PhoneNumber = dto.PhoneNumber;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // =========================
        // UPDATE STATUS
        // =========================
        public async Task UpdateStatusAsync(int userId, bool isActive)
        {
            var user = await _context.Users.FindAsync(userId)
                ?? throw new Exception("User not found");

            user.IsActive = isActive;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // =========================
        // PASSWORD HASH

    }
}
