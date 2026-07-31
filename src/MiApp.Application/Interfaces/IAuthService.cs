using MiApp.Application.Dtos;
using MiApp.APplication.Dtos;

namespace MiApp.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync (LoginRequestDto request);
}