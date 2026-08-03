using MiApp.Domain.Entities;
using MiApp.Domain.Interfaces;

namespace MiApp.Infrastructure.Repositories;


public class CategoryRepositoryMemory : ICategoryRepository
{
    // Simulación de base de datos en memoria
    private static List<Category> _categories = new();
    private static int _nextId = 1;

    public Task<IEnumerable<Category>> GetAllAsync()
    {
        return Task.FromResult(_categories.AsEnumerable());
    }

    public Task<Category?> GetByIdAsync(int id)
    {
        var category = _categories.FirstOrDefault(c => c.Id == id);
        return Task.FromResult(category);
    }

    public Task<Category> CreateAsync(Category category)
    {
        category.Id = _nextId++;
        _categories.Add(category);
        return Task.FromResult(category);
    }

    public Task UpdateAsync(Category category)
    {
        var existing = _categories.FirstOrDefault(c => c.Id == category.Id);
        if(existing == null) return Task.FromResult<Category?>(null);

        existing.Name = category.Name;
        existing.Description = category.Description;
        existing.IsActive = category.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;


        return Task.FromResult<Category?>(existing);
    }

    public Task DeleteAsync(int id)
    {
        var category = _categories.FirstOrDefault(c => c.Id == id);
        if(category == null) return Task.FromResult(false);

        _categories.Remove(category);
        return Task.FromResult(true);
    }

}