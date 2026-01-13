using PrivateECommerce.API.DTOs;
using PrivateECommerce.API.DTOs.Reports;



namespace PrivateECommerce.API.Services
{
    public interface IUserService
    {
        void CreateCustomer(CreateUserDto dto);
        IEnumerable<UserSummaryDto> GetAllUsers();
        UserDetailsDto GetUserDetails(int userId);
        UserPurchaseInsightDto GetUserPurchaseInsights(int userId);
        UserProfileDto GetProfile(int userId);
        void UpdateProfile(int userId, UpdateUserProfileDto dto);
        void ChangePassword(int userId, ChangePasswordDto dto);
    }
}
