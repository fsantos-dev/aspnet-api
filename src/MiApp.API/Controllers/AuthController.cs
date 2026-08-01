using Microsoft.AspNetCore.Mvc;
using MiApp.Application.Dtos;
using MiApp.Application.Interfaces;
using Microsoft.AspNetCore.Identity.Data;
using MiApp.APplication.Dtos;

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
        if(!ModelState.IsValid) return BadRequest(ModelState);

        //Autenticar
        var response = await _authService.LoginAsync(request);

        if(response == null) return Unauthorized("Credenciales Invalidas");

        //Si son válidas, devolver el token

        return Ok(response);
    }
}