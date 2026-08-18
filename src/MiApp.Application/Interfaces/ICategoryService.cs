using MiApp.Application.Dtos;

namespace MiApp.Application.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllAsync();
    Task<CategoryDto> GetByIdAsync(int id);
    Task<CategoryDto> CreateAsync(CreateCategoryDto createDto);
    Task<CategoryDto> UpdateAsync(int id, UpdateCategoryDto updateDto);
    Task DeleteAsync(int id);
}
