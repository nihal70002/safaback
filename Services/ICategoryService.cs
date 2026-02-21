using ClientEcommerce.API.DTOs;

namespace ClientEcommerce.API.Services
{
    public interface ICategoryService
    {
        IEnumerable<CategoryDto> GetAll(bool admin);
        void Create(CreateCategoryDto dto);
        void Update(int id, UpdateCategoryDto dto);
    }
}
