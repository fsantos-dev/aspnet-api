using MiApp.Application.Dtos;
using MiApp.Application.Interfaces;
using MiApp.Domain.Entities;
using MiApp.Domain.Interfaces;

namespace MiApp.Application.Services;

/// <summary>
/// Implementación de la lógica de negocio para categorías.
/// Usa el repositorio (abstracción/interface) para acceder a los datos.
/// </summary>
/// 
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;

    public CategoryService(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var categories = await _repository.GetAllAsync();
        return categories.Select(c => MapToDto(c));
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _repository.GetByIdAsync(id);
        return category == null ? null : MapToDto(category);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto createDto)
    {
    
        var category = new Category(
            createDto.Name,
            createDto.Description
        );

        // 🔹 Guardar usando el repositorio
        var created = await _repository.CreateAsync(category);
        return MapToDto(created);

    }

    public async Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryDto updateDto)
    {
      
        // 🔹 Buscar la entidad existente
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return null;

        // 🔹 Actualizar los campos permitidos
        existing.Rename(updateDto.Name);
        existing.Description = updateDto.Description;
        existing.UpdatedAt = DateTime.UtcNow;

         // 🔹 Guardar cambios usando el repositorio
         var updated = await  _repository.UpdateAsync(existing);
         return updated == null ? null : MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }


    private static CategoryDto MapToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
        };
    }

}