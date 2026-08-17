using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MiApp.Application.Dtos;
using MiApp.Domain.Entities;
using MiApp.Domain.Interfaces;
using MiApp.Application.Interfaces;
using AutoMapper;
using FluentValidation;

namespace MiApp.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    private ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;

    private readonly IValidator<RegisterRequestDto> _registerValidator;

    private readonly IValidator<LoginRequestDto> _loginValidator;

    public AuthService(IUserRepository userRepository, IConfiguration configuracion,
    IMapper mapper, IValidator<RegisterRequestDto> registerValidator, IValidator<LoginRequestDto> loginValidator, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _configuration = configuracion;
        _mapper = mapper;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {

        var validationResult = await _loginValidator.ValidateAsync(request);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);
        // 1. Buscar usuario por email usando el repositorio
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null) throw new InvalidCredentialsException("Credenciales inválidas");

        // 2. Validar contraseña (en memoria, comparación directa. En BD usaríamos hash)
        if (!_passwordHasher.Verify(request.Password, user.PasswordHash)) throw new InvalidCredentialsException("Credenciales inválidas");

        // 3. Generar token JWT
        var token = _tokenService.GenerateToken(user);


        //4. Devolver la respuesta


        //MAPEO MANUAL
        // return new LoginResponseDto
        // {
        //     Token = token,
        //     Email = user.Email,
        //     FullName = user.FullName,
        //     ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "15"))
        // };

        //AUTOMAPPER
        var response = _mapper.Map<LoginResponseDto>(user);

        response.Token = token;
        response.ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "15"));

        return response;


    }

    public async Task<LoginResponseDto?> RegisterAsync(RegisterRequestDto request)
    {
        var validationResult = await _registerValidator.ValidateAsync(request);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

        //1. Validar que el email no este registrado
        var userExisting = await _userRepository.GetByEmailAsync(request.Email);
        if (userExisting != null) throw new InvalidOperationException($"El email '{request.Email}' ya está registrado");

        //2. Crear el usuario
        var user = new User
        {
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            FullName = request.FullName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        //3. Guardar en la base de datos
        var created = await _userRepository.CreateAsync(user);

        //4. Generar el token JWT (Login automatico despues del registro)
        var token = _tokenService.GenerateToken(created);

        //5. Devolver la respuesta


        //MAPEO MANUAL
        // return new LoginResponseDto
        // {
        //     Token = token,
        //     Email = created.Email,
        //     FullName = created.FullName,
        //     ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "15"))
        // };

        //AUTOMAPPER
        var response = _mapper.Map<LoginResponseDto>(created);

        response.Token = token;
        response.ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "15"));

        return response;

    }




}