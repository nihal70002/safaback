using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PrivateECommerce.API.Data;
using PrivateECommerce.API.DTOs;
using PrivateECommerce.API.DTOs.Admin;
using PrivateECommerce.API.DTOs.Reports;
using PrivateECommerce.API.Enum;
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
                throw new Exception("A user with this email already exists.");

            if (_context.Users.Any(u => u.PhoneNumber == dto.PhoneNumber))
                throw new Exception("A user with this phone number already exists.");

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                CompanyName = dto.CompanyName,
                Role = "Customer",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IsActive = true
            };

            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public User CreateSalesExecutive(CreateSalesExecutiveDto dto)
        {
            if (_context.Users.Any(u => u.Email == dto.Email))
                throw new Exception("Email already exists.");

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                CompanyName = dto.CompanyName,
                Role = "SalesExecutive",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IsActive = true
            };

            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }

        public IEnumerable<UserSummaryDto> GetAllUsers()
        {
            return _context.Users
                .AsNoTracking()
                .Where(u => u.Role == "Customer")
                .Include(u => u.OrdersPlaced)
                .OrderBy(u => u.Name) // ✅ backend sorting
                .Select(u => new UserSummaryDto
                {
                    UserId = u.Id,
                    Name = u.Name,
                    Email = u.Email,

                    // ✅ MAP THESE
                    CompanyName = u.CompanyName,
                    PhoneNumber = u.PhoneNumber,

                    TotalOrders = u.OrdersPlaced.Count,
                    TotalSpent = u.OrdersPlaced.Sum(o => (decimal?)o.TotalAmount) ?? 0
                })
                .ToList();
        }


        public UserDetailsDto? GetUserDetails(int userId)
        {
            var user = _context.Users
                .Include(u => u.OrdersPlaced)
                .FirstOrDefault(u => u.Id == userId);

            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} was not found.");

            return new UserDetailsDto
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                OrderHistory = user.OrdersPlaced.Select(o => new OrderHistoryDto
                {
                    OrderId = o.Id,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status
                }).ToList()
            };
        }
        public void AssignSalesExecutiveToCustomer(int customerId, int salesExecutiveId)
        {
            var customer = _context.Users
                .FirstOrDefault(u => u.Id == customerId && u.Role == "Customer");

            if (customer == null)
                throw new Exception("Customer not found");

            var salesExecutive = _context.Users
                .FirstOrDefault(u => u.Id == salesExecutiveId && u.Role == "SalesExecutive");

            if (salesExecutive == null)
                throw new Exception("Sales Executive not found");

            customer.SalesExecutiveId = salesExecutiveId;
            _context.SaveChanges();
        }

        public UserProfileDto GetProfile(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user == null)
                throw new KeyNotFoundException("Profile not found. The user may have been deleted.");

            return new UserProfileDto
            {
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };
        }

        public void CreateCustomerByAdmin(CreateCustomerByAdminDto dto)
        {
            if (_context.Users.Any(u => u.Email == dto.Email))
                throw new Exception("Email is already taken.");

            var salesExecutiveExists = _context.Users.Any(u =>
                u.Id == dto.SalesExecutiveId &&
                u.Role == "SalesExecutive");

            if (!salesExecutiveExists)
                throw new Exception("Invalid Sales Executive");

            var customer = new User
            {
                Name = dto.Name,
                CompanyName = dto.CompanyName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "Customer",
                SalesExecutiveId = dto.SalesExecutiveId,
                IsActive = true
            };

            _context.Users.Add(customer);
            _context.SaveChanges();
        }
        public IEnumerable<SalesExecutiveListDto> GetSalesExecutives(string? search)
        {
            var query = _context.Users
                .Where(u => u.Role == "SalesExecutive" && u.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.Name.Contains(search) ||
                    u.Email.Contains(search));
            }

            return query
                .Select(u => new SalesExecutiveListDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email
                })
                .OrderBy(u => u.Name)
                .ToList();
        }



        public void CreateCustomerBySales(CreateCustomerBySalesDto dto, int salesExecutiveId)
        {
            // Verify sales executive exists first
            if (!_context.Users.Any(u => u.Id == salesExecutiveId))
                throw new Exception("Invalid Sales Executive ID.");

            if (_context.Users.Any(u => u.Email == dto.Email))
                throw new Exception("This customer email is already registered.");

            var customer = new User
            {
                Name = dto.Name,
                CompanyName = dto.CompanyName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "Customer",
                SalesExecutiveId = salesExecutiveId,
                IsActive = true
            };

            _context.Users.Add(customer);
            _context.SaveChanges();
        }

        public List<CustomerDto> GetCustomersForSalesExecutive(int salesExecutiveId)
        {
            return _context.Users
                .AsNoTracking()
                .Where(u => u.Role == "Customer" && u.SalesExecutiveId == salesExecutiveId)
                .Select(u => new CustomerDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    CompanyName = u.CompanyName ?? "",
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber
                })
                .ToList();
        }

        public SalesExecutivePerformanceDto GetSalesExecutivePerformance(int salesExecutiveId)
        {
            var salesExecutive = _context.Users
                .AsNoTracking()
                .FirstOrDefault(u => u.Id == salesExecutiveId && u.Role == "SalesExecutive");

            if (salesExecutive == null)
                throw new KeyNotFoundException("Sales Executive record not found.");

            var orders = _context.Orders.Where(o => o.SalesExecutiveId == salesExecutiveId);

            return new SalesExecutivePerformanceDto
            {
                SalesExecutiveId = salesExecutive.Id,
                Name = salesExecutive.Name,
                TotalCustomers = _context.Users.Count(c => c.Role == "Customer" && c.SalesExecutiveId == salesExecutiveId),
                TotalOrders = orders.Count(),
                AcceptedOrders = orders.Count(o => o.Status == "Accepted"),
                RejectedOrders = orders.Count(o => o.Status == "Rejected"),
                PendingOrders = orders.Count(o => o.Status == "Pending"),
                TotalOrderValue = orders.Where(o => o.Status == "Accepted").Sum(o => (decimal?)o.TotalAmount) ?? 0,
                LastOrderDate = orders.OrderByDescending(o => o.OrderDate).Select(o => (DateTime?)o.OrderDate).FirstOrDefault()
            };
        }

        public void UpdateProfile(int userId, UpdateUserProfileDto dto)
        {
            var user = _context.Users.Find(userId);
            if (user == null) throw new KeyNotFoundException("User not found.");

            user.Name = dto.Name;
            _context.SaveChanges();
        }

        public void ChangePassword(int userId, ChangePasswordDto dto)
        {
            var user = _context.Users.Find(userId);
            if (user == null) throw new KeyNotFoundException("User not found.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            _context.SaveChanges();
        }
        public void UpdateSalesExecutive(int salesExecutiveId, UpdateSalesExecutiveDto dto)
        {
            var salesExecutive = _context.Users
                .FirstOrDefault(u => u.Id == salesExecutiveId && u.Role == "SalesExecutive");

            if (salesExecutive == null)
                throw new Exception("Sales Executive not found");

            // UNIQUE EMAIL
            if (_context.Users.Any(u => u.Email == dto.Email && u.Id != salesExecutiveId))
                throw new Exception("Email already exists");

            // UNIQUE PHONE
            if (_context.Users.Any(u => u.PhoneNumber == dto.PhoneNumber && u.Id != salesExecutiveId))
                throw new Exception("Phone number already exists");

            salesExecutive.Name = dto.Name;
            salesExecutive.Email = dto.Email;
            salesExecutive.PhoneNumber = dto.PhoneNumber;
            salesExecutive.CompanyName = dto.CompanyName;
            salesExecutive.IsActive = dto.IsActive;

            // PASSWORD (ADMIN ONLY)
            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                var hasher = new PasswordHasher<User>();
                salesExecutive.PasswordHash =
                    hasher.HashPassword(salesExecutive, dto.NewPassword);
            }

            _context.SaveChanges();
        }
        // ============================
        // DELETE SALES EXECUTIVE
        // ============================
        public void DeleteSalesExecutive(int salesExecutiveId)
        {
            var salesExecutive = _context.Users
                .FirstOrDefault(u => u.Id == salesExecutiveId && u.Role == "SalesExecutive");

            if (salesExecutive == null)
                throw new Exception("Sales Executive not found");

            _context.Users.Remove(salesExecutive);
            _context.SaveChanges();
        }


        public IEnumerable<SalesExecutiveAdminSummaryDto> GetAllSalesExecutivesForAdmin()
        {
            return _context.Users
                .AsNoTracking()
                .Where(u => u.Role == "SalesExecutive")
                .Select(u => new SalesExecutiveAdminSummaryDto
                {
                    SalesExecutiveId = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    CompanyName = u.CompanyName,
                    PhoneNumber = u.PhoneNumber,
                    TotalCustomers = _context.Users.Count(c => c.Role == "Customer" && c.SalesExecutiveId == u.Id),
                    TotalOrders = _context.Orders.Count(o => o.SalesExecutiveId == u.Id),
                    AcceptedOrders = _context.Orders.Count(o => o.SalesExecutiveId == u.Id && o.Status == "Accepted"),
                    RejectedOrders = _context.Orders.Count(o => o.SalesExecutiveId == u.Id && o.Status == "Rejected"),
                    PendingOrders = _context.Orders.Count(o => o.SalesExecutiveId == u.Id && o.Status == "Pending"),
                    TotalOrderValue = _context.Orders.Where(o => o.SalesExecutiveId == u.Id && o.Status == "Accepted").Sum(o => (decimal?)o.TotalAmount) ?? 0
                })
                .ToList();
        }

        public UserPurchaseInsightDto GetUserPurchaseInsights(int userId)
            => new UserPurchaseInsightDto();
    }
}