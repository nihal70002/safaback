using PrivateECommerce.API.Data;
using PrivateECommerce.API.DTOs;
using PrivateECommerce.API.Models;

namespace PrivateECommerce.API.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<CategoryDto> GetAll(bool admin)
        {
            var query = _context.Categories.AsQueryable();

            if (!admin)
                query = query.Where(c => c.IsActive);

            return query
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    IsActive = c.IsActive
                })
                .ToList();
        }

        public void Create(CreateCategoryDto dto)
        {
            if (_context.Categories.Any(c => c.Name == dto.Name))
                throw new Exception("Category already exists");

            _context.Categories.Add(new Category
            {
                Name = dto.Name,
                IsActive = true
            });

            _context.SaveChanges();
        }

        public void Update(int id, UpdateCategoryDto dto)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
                throw new Exception("Category not found");

            category.Name = dto.Name;
            category.IsActive = dto.IsActive;

            _context.SaveChanges();
        }
    }
}
