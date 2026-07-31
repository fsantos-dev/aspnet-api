namespace MiApp.Application.Dtos;

/// DTO para devolver el token JWT y la información básica del usuario después del login.
public class LoginResponseDto
{
    public string Token {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
    public string? FullName {get; set;}
    public DateTime ExpiresAt {get;set;}
}