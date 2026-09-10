using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MiApp.API.Controllers;
using MiApp.Application.Dtos;
using MiApp.Application.Interfaces;

namespace MiApp.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;

    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();

        _controller = new AuthController(
            _authServiceMock.Object
        );
    }


    // ============================================================
    // LOGIN
    // ============================================================

    [Fact]
    public async Task Login_ShouldReturnOk_WhenCredentialsAreValid()
    {
        // Arrange

        var request = new LoginRequestDto
        {
            Email = "test@email.com",
            Password = "Password123"
        };

        var response = new LoginResponseDto
        {
            Token = "jwt-token",
            Email = request.Email,
            FullName = "Juan Perez"
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(request))
            .ReturnsAsync(response);

        // Act

        var result = await _controller.Login(request);

        // Assert

        // ¿El controller devuelve 200 OK?
        var okResult = result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        // ¿Devuelve la respuesta correcta?
        okResult.Value.Should().BeEquivalentTo(response);

        // ¿Se llamó al service con el request correcto?
        _authServiceMock.Verify(
            x => x.LoginAsync(request),
            Times.Once
        );
    }


    // ============================================================
    // REGISTER
    // ============================================================

    [Fact]
    public async Task Register_ShouldReturnOk_WhenModelStateIsValid()
    {
        // Arrange

        var request = new RegisterRequestDto
        {
            Email = "new@email.com",
            Password = "Password123",
            FullName = "Juan Perez"
        };

        var response = new LoginResponseDto
        {
            Token = "jwt-token",
            Email = request.Email,
            FullName = request.FullName
        };

        _authServiceMock
            .Setup(x => x.RegisterAsync(request))
            .ReturnsAsync(response);

        // Act

        var result = await _controller.Register(request);

        // Assert

        // ¿El controller devuelve 200 OK?
        var okResult = result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        // ¿Devuelve la respuesta correcta?
        okResult.Value.Should().BeEquivalentTo(response);

        // ¿Se llamó al service?
        _authServiceMock.Verify(
            x => x.RegisterAsync(request),
            Times.Once
        );
    }


    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange

        var request = new RegisterRequestDto
        {
            Email = "",
            Password = "",
            FullName = ""
        };

        _controller.ModelState.AddModelError(
            "Email",
            "El email es obligatorio"
        );

        // Act

        var result = await _controller.Register(request);

        // Assert

        // ¿El controller devuelve 400 Bad Request?
        result.Should()
            .BeOfType<BadRequestObjectResult>();

        // ¿El service NO debe ejecutarse?
        _authServiceMock.Verify(
            x => x.RegisterAsync(It.IsAny<RegisterRequestDto>()),
            Times.Never
        );
    }
}