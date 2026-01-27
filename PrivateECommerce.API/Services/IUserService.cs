using PrivateECommerce.API.DTOs;
using PrivateECommerce.API.DTOs.Admin;
using PrivateECommerce.API.DTOs.Reports;
using PrivateECommerce.API.Models;
using PrivateECommerce.API.DTOs.Admin;

namespace PrivateECommerce.API.Services
{
    public interface IUserService
    {
        // ============================
        // ADMIN
        // ============================
        void CreateCustomer(CreateUserDto dto);
        List<AdminCustomerDto> GetAllUsers();

        UserDetailsDto? GetUserDetails(int userId);

        IEnumerable<SalesExecutiveAdminSummaryDto> GetAllSalesExecutivesForAdmin();
        User CreateSalesExecutive(CreateSalesExecutiveDto dto);
        SalesExecutivePerformanceDto GetSalesExecutivePerformance(int salesExecutiveId);

        void CreateCustomerByAdmin(CreateCustomerByAdminDto dto);

        // ============================
        // CUSTOMER
        // ============================
        UserProfileDto GetProfile(int userId);
        void UpdateProfile(int userId, UpdateUserProfileDto dto);
        void ChangePassword(int userId, ChangePasswordDto dto);
        IEnumerable<SalesExecutiveListDto> GetSalesExecutives(string? search);

        // ============================
        // SALES EXECUTIVE
        // ============================
        void CreateCustomerBySales(
            CreateCustomerBySalesDto dto,
            int salesExecutiveId
        );
        void UpdateSalesExecutive(int salesExecutiveId, UpdateSalesExecutiveDto dto);
        void AssignSalesExecutiveToCustomer(int customerId, int salesExecutiveId);

        // ✅ DELETE
        void DeleteSalesExecutive(int salesExecutiveId);

        List<CustomerDto> GetCustomersForSalesExecutive(int salesExecutiveId);

        // ============================
        // REPORTS
        // ============================
        UserPurchaseInsightDto GetUserPurchaseInsights(int userId);
    }
}
