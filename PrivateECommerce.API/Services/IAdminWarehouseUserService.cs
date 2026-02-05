using PrivateECommerce.API.DTOs;

namespace PrivateECommerce.API.Services
{
    public interface IAdminWarehouseUserService
    {
        Task CreateAsync(CreateWarehouseUserDto dto);
        Task<IEnumerable<object>> GetAllAsync();
        Task UpdateAsync(int userId, UpdateWarehouseUserDto dto);
        Task UpdateStatusAsync(int userId, bool isActive);
    }
}
