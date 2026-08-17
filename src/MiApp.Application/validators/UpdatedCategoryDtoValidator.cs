using FluentValidation;
using MiApp.Application.Dtos;

namespace MiApp.Application.validators;

public class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDto>
{
    public UpdateCategoryDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la categoria es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres");
        
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La Descripcion no puede superar los 500 caracteres");
    }
}