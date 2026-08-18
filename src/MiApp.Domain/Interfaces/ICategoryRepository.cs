using MiApp.Domain.Entities;

namespace MiApp.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync(int id);
    Task<Category?> GetByIdAsync(int id, int userId);

    Task<Category> CreateAsync(Category category);

    Task UpdateAsync(Category category);

    Task DeleteAsync(int id);
}

