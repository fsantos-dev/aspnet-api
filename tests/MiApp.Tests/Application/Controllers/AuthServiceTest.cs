using AutoMapper;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using MiApp.Application.Dtos;
using MiApp.Application.Interfaces;
using MiApp.Application.Services;
using MiApp.Domain.Entities;
using MiApp.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MiApp.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<RegisterRequestDto>> _registerValidatorMock;
    private readonly Mock<IValidator<LoginRequestDto>> _loginValidatorMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ITokenService> _tokenServiceMock;

    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _configurationMock = new Mock<IConfiguration>();
        _mapperMock = new Mock<IMapper>();
        _registerValidatorMock = new Mock<IValidator<RegisterRequestDto>>();
        _loginValidatorMock = new Mock<IValidator<LoginRequestDto>>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenServiceMock = new Mock<ITokenService>();

        _configurationMock
            .Setup(x => x["Jwt:ExpiryMinutes"])
            .Returns("15");

        _service = new AuthService(
            _userRepositoryMock.Object,
            _configurationMock.Object,
            _mapperMock.Object,
            _registerValidatorMock.Object,
            _loginValidatorMock.Object,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object
        );
    }


    // ============================================================
    // LOGIN
    // ============================================================

    [Fact]
    public async Task LoginAsync_ShouldReturnLoginResponse_WhenCredentialsAreValid()
    {
        // Arrange

        var request = new LoginRequestDto
        {
            Email = "test@email.com",
            Password = "Password123"
        };

        var user = new User
        {
            Email = request.Email,
            PasswordHash = "hashed-password",
            FullName = "Juan Perez",
            IsActive = true
        };

        var validationResult = new ValidationResult();

        var response = new LoginResponseDto();

        _loginValidatorMock
            .Setup(x => x.ValidateAsync(request, default))
            .ReturnsAsync(validationResult);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(
                request.Password,
                user.PasswordHash))
            .Returns(true);

        _tokenServiceMock
            .Setup(x => x.GenerateToken(user))
            .Returns("jwt-token");

        _mapperMock
            .Setup(x => x.Map<LoginResponseDto>(user))
            .Returns(response);

        // Act

        var result = await _service.LoginAsync(request);

        // Assert

        // ¿Lo que retorna el servicio es correcto?
        result.Should().NotBeNull();
        result!.Token.Should().Be("jwt-token");

        // ¿El repository buscó el usuario correcto?
        _userRepositoryMock.Verify(
            x => x.GetByEmailAsync(request.Email),
            Times.Once
        );

        // ¿Se verificó la contraseña?
        _passwordHasherMock.Verify(
            x => x.Verify(
                request.Password,
                user.PasswordHash),
            Times.Once
        );

        // ¿Se generó el token?
        _tokenServiceMock.Verify(
            x => x.GenerateToken(user),
            Times.Once
        );

        // ¿Se hizo el mapeo?
        _mapperMock.Verify(
            x => x.Map<LoginResponseDto>(user),
            Times.Once
        );
    }


    [Fact]
    public async Task LoginAsync_ShouldThrowValidationException_WhenDataIsInvalid()
    {
        // Arrange

        var request = new LoginRequestDto
        {
            Email = "",
            Password = ""
        };

        var validationResult = new ValidationResult(
            new[]
            {
                new ValidationFailure(
                    "Email",
                    "El email es obligatorio"
                )
            }
        );

        _loginValidatorMock
            .Setup(x => x.ValidateAsync(request, default))
            .ReturnsAsync(validationResult);

        // Act

        Func<Task> act = () => _service.LoginAsync(request);

        // Assert

        await act.Should().ThrowAsync<ValidationException>();

        // No debe buscar el usuario porque la validación falló
        _userRepositoryMock.Verify(
            x => x.GetByEmailAsync(It.IsAny<string>()),
            Times.Never
        );

        // No debe verificar contraseña
        _passwordHasherMock.Verify(
            x => x.Verify(
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never
        );

        // No debe generar token
        _tokenServiceMock.Verify(
            x => x.GenerateToken(It.IsAny<User>()),
            Times.Never
        );
    }


    [Fact]
    public async Task LoginAsync_ShouldThrowInvalidCredentialsException_WhenUserDoesNotExist()
    {
        // Arrange

        var request = new LoginRequestDto
        {
            Email = "unknown@email.com",
            Password = "Password123"
        };

        var validationResult = new ValidationResult();

        _loginValidatorMock
            .Setup(x => x.ValidateAsync(request, default))
            .ReturnsAsync(validationResult);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        // Act

        Func<Task> act = () => _service.LoginAsync(request);

        // Assert

        await act.Should().ThrowAsync<InvalidCredentialsException>();

        _userRepositoryMock.Verify(
            x => x.GetByEmailAsync(request.Email),
            Times.Once
        );

        // No debe verificar contraseña porque el usuario no existe
        _passwordHasherMock.Verify(
            x => x.Verify(
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never
        );

        // No debe generar token
        _tokenServiceMock.Verify(
            x => x.GenerateToken(It.IsAny<User>()),
            Times.Never
        );
    }


    [Fact]
    public async Task LoginAsync_ShouldThrowInvalidCredentialsException_WhenPasswordIsIncorrect()
    {
        // Arrange

        var request = new LoginRequestDto
        {
            Email = "test@email.com",
            Password = "WrongPassword"
        };

        var validationResult = new ValidationResult();

        var user = new User
        {
            Email = request.Email,
            PasswordHash = "hashed-password",
            FullName = "Juan Perez",
            IsActive = true
        };

        _loginValidatorMock
            .Setup(x => x.ValidateAsync(request, default))
            .ReturnsAsync(validationResult);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(
                request.Password,
                user.PasswordHash))
            .Returns(false);

        // Act

        Func<Task> act = () => _service.LoginAsync(request);

        // Assert

        await act.Should().ThrowAsync<InvalidCredentialsException>();

        _userRepositoryMock.Verify(
            x => x.GetByEmailAsync(request.Email),
            Times.Once
        );

        _passwordHasherMock.Verify(
            x => x.Verify(
                request.Password,
                user.PasswordHash),
            Times.Once
        );

        // No debe generar token si la contraseña es incorrecta
        _tokenServiceMock.Verify(
            x => x.GenerateToken(It.IsAny<User>()),
            Times.Never
        );
    }


    // ============================================================
    // REGISTER
    // ============================================================

    [Fact]
    public async Task RegisterAsync_ShouldCreateUserAndReturnLoginResponse_WhenDataIsValid()
    {
        // Arrange

        var request = new RegisterRequestDto
        {
            Email = "new@email.com",
            Password = "Password123",
            FullName = "Juan Perez"
        };

        var validationResult = new ValidationResult();

        var createdUser = new User
        {
            Id = 1,
            Email = request.Email,
            PasswordHash = "hashed-password",
            FullName = request.FullName,
            IsActive = true
        };

        var response = new LoginResponseDto();

        _registerValidatorMock
            .Setup(x => x.ValidateAsync(request, default))
            .ReturnsAsync(validationResult);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        _passwordHasherMock
            .Setup(x => x.Hash(request.Password))
            .Returns("hashed-password");

        _userRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(createdUser);

        _tokenServiceMock
            .Setup(x => x.GenerateToken(createdUser))
            .Returns("jwt-token");

        _mapperMock
            .Setup(x => x.Map<LoginResponseDto>(createdUser))
            .Returns(response);

        // Act

        var result = await _service.RegisterAsync(request);

        // Assert

        // ¿Lo que retorna el servicio es correcto?
        result.Should().NotBeNull();
        result!.Token.Should().Be("jwt-token");

        // ¿Se buscó si el email ya estaba registrado?
        _userRepositoryMock.Verify(
            x => x.GetByEmailAsync(request.Email),
            Times.Once
        );

        // ¿Se hasheó la contraseña?
        _passwordHasherMock.Verify(
            x => x.Hash(request.Password),
            Times.Once
        );

        // ¿El repository recibió el usuario correcto?
        _userRepositoryMock.Verify(
            x => x.CreateAsync(It.Is<User>(u =>
                u.Email == request.Email &&
                u.PasswordHash == "hashed-password" &&
                u.FullName == request.FullName &&
                u.IsActive
            )),
            Times.Once
        );

        // ¿Se generó el token?
        _tokenServiceMock.Verify(
            x => x.GenerateToken(createdUser),
            Times.Once
        );

        // ¿Se hizo el mapeo?
        _mapperMock.Verify(
            x => x.Map<LoginResponseDto>(createdUser),
            Times.Once
        );
    }


    [Fact]
    public async Task RegisterAsync_ShouldThrowValidationException_WhenDataIsInvalid()
    {
        // Arrange

        var request = new RegisterRequestDto
        {
            Email = "",
            Password = "",
            FullName = ""
        };

        var validationResult = new ValidationResult(
            new[]
            {
                new ValidationFailure(
                    "Email",
                    "El email es obligatorio"
                )
            }
        );

        _registerValidatorMock
            .Setup(x => x.ValidateAsync(request, default))
            .ReturnsAsync(validationResult);

        // Act

        Func<Task> act = () => _service.RegisterAsync(request);

        // Assert

        await act.Should().ThrowAsync<ValidationException>();

        // No debe buscar el email
        _userRepositoryMock.Verify(
            x => x.GetByEmailAsync(It.IsAny<string>()),
            Times.Never
        );

        // No debe hashear
        _passwordHasherMock.Verify(
            x => x.Hash(It.IsAny<string>()),
            Times.Never
        );

        // No debe crear usuario
        _userRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<User>()),
            Times.Never
        );

        // No debe generar token
        _tokenServiceMock.Verify(
            x => x.GenerateToken(It.IsAny<User>()),
            Times.Never
        );
    }


    [Fact]
    public async Task RegisterAsync_ShouldThrowInvalidOperationException_WhenEmailIsAlreadyRegistered()
    {
        // Arrange

        var request = new RegisterRequestDto
        {
            Email = "existing@email.com",
            Password = "Password123",
            FullName = "Juan Perez"
        };

        var validationResult = new ValidationResult();

        var existingUser = new User
        {
            Id = 1,
            Email = request.Email,
            PasswordHash = "existing-hash",
            FullName = "Existing User",
            IsActive = true
        };

        _registerValidatorMock
            .Setup(x => x.ValidateAsync(request, default))
            .ReturnsAsync(validationResult);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        // Act

        Func<Task> act = () => _service.RegisterAsync(request);

        // Assert

        await act.Should().ThrowAsync<InvalidOperationException>();

        _userRepositoryMock.Verify(
            x => x.GetByEmailAsync(request.Email),
            Times.Once
        );

        // No debe hashear porque el email ya existe
        _passwordHasherMock.Verify(
            x => x.Hash(It.IsAny<string>()),
            Times.Never
        );

        // No debe crear otro usuario
        _userRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<User>()),
            Times.Never
        );

        // No debe generar token
        _tokenServiceMock.Verify(
            x => x.GenerateToken(It.IsAny<User>()),
            Times.Never
        );
    }
}