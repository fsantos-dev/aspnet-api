using FluentValidation;
using MiApp.Application.Dtos;
using MiApp.Domain.Entities;


namespace MiApp.Application.validators;


public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .EmailAddress().WithMessage("El formato del email no es valido")
            .MaximumLength(150).WithMessage("El nombre no puede superar los 150 caracteres");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contrasena es obligatoria.")
            .MaximumLength(200).WithMessage("La contrasena supera la longitud permitida.");
    }   
}