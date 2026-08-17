using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MiApp.Application.Interfaces;
using MiApp.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MiApp.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {

        // Leer la configuración desde appsettings.json
        var secretKey = _configuration["Jwt:SecretKey"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(GetExpiryMinutes()),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public int GetExpiryMinutes()
    {
        return int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "15");
    }
}