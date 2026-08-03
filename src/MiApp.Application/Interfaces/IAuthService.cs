using MiApp.Application.Dtos;

namespace MiApp.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync (LoginRequestDto request);
    Task<LoginResponseDto?> RegisterAsync(RegisterRequestDto request);
}