namespace MiApp.Application.Dtos;

/// DTO para recibir las credenciales de login desde el cliente (Angular, Postman, etc.)

public class LoginRequestDto
{
    public string Email {get; set;} = string.Empty;
    public string Password {get; set;} = string.Empty;
}