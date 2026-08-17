using FluentValidation;
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
    private readonly IValidator<CreateCategoryDto> _createValidator;
    private readonly IValidator<UpdateCategoryDto> _updatedValidator;

    public CategoryService(ICategoryRepository repository, IValidator<CreateCategoryDto> createValidator, IValidator<UpdateCategoryDto> updateValidator)
    {
        _repository = repository;
        _createValidator = createValidator;
        _updatedValidator = updateValidator;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync(int id)
    {
        var categories = await _repository.GetAllAsync(id);
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

        var validationResult = await _createValidator.ValidateAsync(createDto);
        if(!validationResult.IsValid) throw new ValidationException(validationResult.Errors);
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

        var validationResult = await _updatedValidator.ValidateAsync(updateDto);
        if(!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

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