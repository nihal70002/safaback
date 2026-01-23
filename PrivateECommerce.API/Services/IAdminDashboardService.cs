using PrivateECommerce.API.DTOs.Admin;

namespace PrivateECommerce.API.Services
{
    public interface IAdminDashboardService
    {
        AdminDashboardSummaryDto GetSummary();
    }
}
