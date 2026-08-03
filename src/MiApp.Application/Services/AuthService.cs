using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MiApp.Application.Dtos;
using MiApp.Domain.Entities;
using MiApp.Domain.Interfaces;
using MiApp.Application.Interfaces;

namespace MiApp.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IConfiguration configuracion)
    {
        _userRepository = userRepository;
        _configuration = configuracion;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        // 1. Buscar usuario por email usando el repositorio
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null) throw new InvalidCredentialsException("Credenciales inválidas");

        // 2. Validar contraseña (en memoria, comparación directa. En BD usaríamos hash)
        if (!VerifyPassword(request.Password, user.PasswordHash)) throw new InvalidCredentialsException("Credenciales inválidas");

        // 3. Generar token JWT
        var token = GenerateJwtToken(user);

        return new LoginResponseDto
        {
            Token = token,
            Email = user.Email,
            FullName = user.FullName,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "15"))
        };
    }

    public async Task<LoginResponseDto?> RegisterAsync(RegisterRequestDto request)
    {

        //1. Validar que el email no este registrado
        var userExisting = await _userRepository.GetByEmailAsync(request.Email);
        if (userExisting != null) throw new InvalidOperationException($"El email '{request.Email}' ya está registrado");

        //2. Crear el usuario
        var user = new User
        {
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            FullName = request.FullName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        //3. Guardar en la base de datos
        var created = await _userRepository.CreateAsync(user);

        //4. Generar el token JWT (Login automatico despues del registro)
        var token = GenerateJwtToken(created);

        //5. Devolver la respuesta
        return new LoginResponseDto
        {
            Token = token,
            Email = created.Email,
            FullName = created.FullName,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "15"))
        };

    }

    private bool VerifyPassword(string password, string passwordHash)
    {
        return HashPassword(password) == passwordHash;
    }

    private string HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }


    private string GenerateJwtToken(User user)
    {

        // Leer la configuración desde appsettings.json
        var secretKey = _configuration["Jwt:SecretKey"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "15");

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
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}