using FluentValidation;
using MiApp.Application.Dtos;

namespace MiApp.Application.validators;

public class RegisterRequestDtoValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .EmailAddress().WithMessage("El formato del email no es valido")
            .MaximumLength(150).WithMessage("El nombre no puede superar los 150 caracteres");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contrasena es obligatoria.")
            .MinimumLength(8).WithMessage("La contrasena debe tener almenos 8 caracteres")
            .MaximumLength(50).WithMessage("La contrasena no debe superar los 50 caracteres");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contrasena es obligatoria.")
            .MinimumLength(8).WithMessage("La contrasena debe tener almenos 8 caracteres")
            .MaximumLength(50).WithMessage("La contrasena no debe superar los 50 caracteres");

        RuleFor(x => x.FullName)
            .MaximumLength(100).WithMessage("El nombre completo no puede superar los 100 caracteres");
    }
}