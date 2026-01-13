using Microsoft.EntityFrameworkCore;
using PrivateECommerce.API.Data;
using PrivateECommerce.API.DTOs;
using PrivateECommerce.API.Models;
using PrivateECommerce.API.DTOs.Reports;


namespace PrivateECommerce.API.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        // 1. Create User Logic
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

        // 2. Fetch All Users for the Admin Sidebar
        public IEnumerable<UserSummaryDto> GetAllUsers()
        {
            return _context.Users
                .Include(u => u.Orders)
                .Select(u => new UserSummaryDto
                {
                    UserId = u.Id,
                    Name = u.Name, // Matches the 'Name' property in your User model
                    Email = u.Email,
                    TotalOrders = u.Orders.Count(),
                    TotalSpent = u.Orders.Any() ? u.Orders.Sum(o => o.TotalAmount) : 0
                })
                .ToList();
        }

        // 3. Fetch Full Detail + Order History for a specific User
        public UserDetailsDto GetUserDetails(int userId)
        {
            var user = _context.Users
                .Include(u => u.Orders)
                .FirstOrDefault(u => u.Id == userId);

            if (user == null) return null;

            return new UserDetailsDto
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,

                

                TotalOrders = user.Orders.Count,
                TotalSpent = user.Orders.Sum(o => o.TotalAmount),

                OrderHistory = user.Orders
                    .OrderByDescending(o => o.OrderDate)
                    .Select(o => new OrderHistoryDto
                    {
                        OrderId = o.Id,
                        OrderDate = o.OrderDate,   // ✅ FIX
                        Status = o.Status,         // ✅ FIX (MAIN ISSUE)
                        TotalAmount = o.TotalAmount
                    })
                    .ToList()
            };
        }

        public UserPurchaseInsightDto GetUserPurchaseInsights(int userId)
        {
            var deliveredOrders = _context.Orders
                .Where(o => o.UserId == userId && o.Status == "Delivered");

            return new UserPurchaseInsightDto
            {
                TotalOrders = deliveredOrders.Count(),
                TotalSpent = deliveredOrders.Sum(o => o.TotalAmount),
                FavoriteProducts = _context.OrderItems
                    .Where(oi => oi.Order.UserId == userId &&
                                 oi.Order.Status == "Delivered")
                    .GroupBy(oi => oi.ProductVariant.Product.Name)
                    .Select(g => new CustomerProductInterestDto
                    {
                        ProductName = g.Key,
                        QuantityBought = g.Sum(x => x.Quantity),
                        Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
                    })
                    .OrderByDescending(x => x.QuantityBought)
                    .Take(5)
                    .ToList()
            };
        }
        public UserProfileDto GetProfile(int userId)
        {
            var user = _context.Users.First(u => u.Id == userId);

            return new UserProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                CompanyName = user.CompanyName,  // 👈 add
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt
            };
        }


        public void UpdateProfile(int userId, UpdateUserProfileDto dto)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return;

            user.Name = dto.Name;
            user.PhoneNumber = dto.PhoneNumber;
            _context.SaveChanges();
        }

        public void ChangePassword(int userId, ChangePasswordDto dto)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return;

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                throw new Exception("Invalid password");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            _context.SaveChanges();
        }



    }
}