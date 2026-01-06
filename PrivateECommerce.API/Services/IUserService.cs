using PrivateECommerce.API.DTOs;


namespace PrivateECommerce.API.Services
{
    public interface IUserService
    {
        void CreateCustomer(CreateUserDto dto);
    }
}
