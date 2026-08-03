using MiApp.Application.Dtos;
using MiApp.Application.Interfaces;
using MiApp.Application.Mappings;
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
        return categories.Select(c => CategoryMapper.toDto(c));
    }

    public async Task<CategoryDto> GetByIdAsync(int id)
    {
        var category = await _repository.GetByIdAsync(id);
        if (category is null) throw new KeyNotFoundException($"No se encontró la categoría con el ID {id}");
        return CategoryMapper.toDto(category);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto createDto)
    {

        var category = new Category(
            createDto.Name,
            createDto.Description
        );

        // 🔹 Guardar usando el repositorio
        var created = await _repository.CreateAsync(category);
        return CategoryMapper.toDto(created);

    }

    public async Task<CategoryDto> UpdateAsync(int id, UpdateCategoryDto updateDto)
    {

        // 🔹 Buscar la entidad existente
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) throw new KeyNotFoundException($"No se encontró la categoría con el ID {id}");

        // 🔹 Actualizar los campos permitidos
        existing.Rename(updateDto.Name);
        existing.Description = updateDto.Description;
        existing.UpdatedAt = DateTime.UtcNow;

        // 🔹 Guardar cambios usando el repositorio
        await _repository.UpdateAsync(existing);
        return CategoryMapper.toDto(existing);
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _repository.GetByIdAsync(id);
        if (category is null) throw new KeyNotFoundException($"No se encontró la categoría con el ID {id}");
        await _repository.DeleteAsync(id);
    }



}