using MiApp.Application.Dtos;
using MiApp.Domain.Entities;

namespace MiApp.Application.Mappings;

public static class CategoryMapper
{
    //los datos que respone el servidor al cliente
    public static CategoryDto toDto(Category category)
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

    // Los datos que envia el cliente
    public static Category toEntity(CreateCategoryDto dto)
    {
        return new Category
        (
            dto.Name,
            dto.Description
        );
    }
}