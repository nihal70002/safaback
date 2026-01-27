using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PrivateECommerce.API.Data;
using PrivateECommerce.API.DTOs;
using PrivateECommerce.API.DTOs.Admin;
using PrivateECommerce.API.DTOs.Reports;
using PrivateECommerce.API.Models;

namespace PrivateECommerce.API.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public UserService(AppDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        // ============================
        // ADMIN
        // ============================
        public void CreateCustomer(CreateUserDto dto)
        {
            if (_context.Users.Any(u => u.Email == dto.Email))
                throw new Exception("Email already exists.");

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                CompanyName = dto.CompanyName,
                Role = "Customer",
                IsActive = true
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public List<AdminCustomerDto> GetAllUsers()
        {
            return _context.Users
                .AsNoTracking()
                .Include(u => u.SalesExecutive)
                .Where(u => u.Role == "Customer")
                .Select(u => new AdminCustomerDto
                {
                    UserId = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    CompanyName = u.CompanyName,
                    PhoneNumber = u.PhoneNumber,

                    SalesExecutiveId = u.SalesExecutiveId,
                    SalesExecutiveName = u.SalesExecutive != null
                        ? u.SalesExecutive.Name
                        : null
                })
                .ToList();
        }


        public UserDetailsDto? GetUserDetails(int userId)
        {
            var user = _context.Users
                .Include(u => u.OrdersPlaced)
                .FirstOrDefault(u => u.Id == userId);

            if (user == null) return null;

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

        public IEnumerable<SalesExecutiveAdminSummaryDto> GetAllSalesExecutivesForAdmin()
        {
            return _context.Users
                .Where(u => u.Role == "SalesExecutive")
                .Select(u => new SalesExecutiveAdminSummaryDto
                {
                    SalesExecutiveId = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    CompanyName = u.CompanyName,
                    PhoneNumber = u.PhoneNumber
                })
                .ToList();
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
                IsActive = true
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }

        public SalesExecutivePerformanceDto GetSalesExecutivePerformance(int salesExecutiveId)
        {
            var orders = _context.Orders.Where(o => o.SalesExecutiveId == salesExecutiveId);

            return new SalesExecutivePerformanceDto
            {
                SalesExecutiveId = salesExecutiveId,
                TotalOrders = orders.Count(),
                AcceptedOrders = orders.Count(o => o.Status == "Accepted"),
                RejectedOrders = orders.Count(o => o.Status == "Rejected"),
                PendingOrders = orders.Count(o => o.Status == "Pending"),
                TotalOrderValue = orders.Sum(o => (decimal?)o.TotalAmount) ?? 0
            };
        }

        public void CreateCustomerByAdmin(CreateCustomerByAdminDto dto)
        {
            var customer = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                CompanyName = dto.CompanyName,
                Role = "Customer",
                SalesExecutiveId = dto.SalesExecutiveId,
                IsActive = true
            };

            customer.PasswordHash = _passwordHasher.HashPassword(customer, dto.Password);

            _context.Users.Add(customer);
            _context.SaveChanges();
        }

        // ============================
        // CUSTOMER
        // ============================
        public UserProfileDto GetProfile(int userId)
        {
            var user = _context.Users.Find(userId)
                ?? throw new KeyNotFoundException("User not found");

            return new UserProfileDto
            {
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };
        }

        public void UpdateProfile(int userId, UpdateUserProfileDto dto)
        {
            var user = _context.Users.Find(userId)
                ?? throw new KeyNotFoundException("User not found");

            user.Name = dto.Name;
            _context.SaveChanges();
        }

        public void ChangePassword(int userId, ChangePasswordDto dto)
        {
            var user = _context.Users.Find(userId)
                ?? throw new KeyNotFoundException("User not found");

            user.PasswordHash =
                _passwordHasher.HashPassword(user, dto.NewPassword);

            _context.SaveChanges();
        }

        public IEnumerable<SalesExecutiveListDto> GetSalesExecutives(string? search)
        {
            var query = _context.Users
                .Where(u => u.Role == "SalesExecutive");

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.Name.Contains(search) ||
                    u.Email.Contains(search));
            }

            return query.Select(u => new SalesExecutiveListDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email
            }).ToList();
        }

        // ============================
        // SALES EXECUTIVE
        // ============================
        public void CreateCustomerBySales(CreateCustomerBySalesDto dto, int salesExecutiveId)
        {
            var customer = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                CompanyName = dto.CompanyName,
                Role = "Customer",
                SalesExecutiveId = salesExecutiveId,
                IsActive = true
            };

            customer.PasswordHash =
                _passwordHasher.HashPassword(customer, dto.Password);

            _context.Users.Add(customer);
            _context.SaveChanges();
        }

        public void UpdateSalesExecutive(int salesExecutiveId, UpdateSalesExecutiveDto dto)
        {
            var user = _context.Users.Find(salesExecutiveId)
                ?? throw new Exception("Sales Executive not found");

            user.Name = dto.Name;
            user.Email = dto.Email;
            user.PhoneNumber = dto.PhoneNumber;
            user.CompanyName = dto.CompanyName;
            user.IsActive = dto.IsActive;

            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                user.PasswordHash =
                    _passwordHasher.HashPassword(user, dto.NewPassword);
            }

            _context.SaveChanges();
        }

        public void AssignSalesExecutiveToCustomer(int customerId, int salesExecutiveId)
        {
            var customer = _context.Users.Find(customerId)
                ?? throw new Exception("Customer not found");

            customer.SalesExecutiveId = salesExecutiveId;
            _context.SaveChanges();
        }

        public void DeleteSalesExecutive(int salesExecutiveId)
        {
            var user = _context.Users.Find(salesExecutiveId)
                ?? throw new Exception("Sales Executive not found");

            _context.Users.Remove(user);
            _context.SaveChanges();
        }

        public List<CustomerDto> GetCustomersForSalesExecutive(int salesExecutiveId)
        {
            return _context.Users
                .Where(u => u.Role == "Customer" && u.SalesExecutiveId == salesExecutiveId)
                .Select(u => new CustomerDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    CompanyName = u.CompanyName
                })
                .ToList();
        }

        // ============================
        // REPORTS
        // ============================
        public UserPurchaseInsightDto GetUserPurchaseInsights(int userId)
        {
            return new UserPurchaseInsightDto();
        }
    }
}
