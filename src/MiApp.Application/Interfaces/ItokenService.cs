using MiApp.Domain.Entities;

namespace MiApp.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
    int GetExpiryMinutes();
}