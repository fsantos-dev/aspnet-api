using Microsoft.AspNetCore.Mvc;
using MiApp.Application.Dtos;
using MiApp.Application.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MiApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }   

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        //Autenticar
        var response = await _authService.LoginAsync(request);

        //Si son válidas, devolver el token
        return Ok(response);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _authService.RegisterAsync(request);

        return Ok(result);
    }
}