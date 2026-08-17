using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MiApp.API.Middleware;

/// <summary>
/// Handler global de excepciones. Captura cualquier excepción no controlada
/// que ocurra en el pipeline y devuelve una respuesta consistente en formato ProblemDetails.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Ocurrió una excepción no controlada: {Message}", exception.Message);

        var (statusCode, title, detail) = exception switch
        {
            ArgumentException or ValidationException => (
                HttpStatusCode.BadRequest,
                "Solicitud inválida",
                exception.Message),

            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                "Recurso no encontrado",
                exception.Message),

            InvalidOperationException => (
                HttpStatusCode.Conflict,
                "Conflicto",
                exception.Message),

            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                "No autorizado",
                "No tienes permisos para realizar esta acción."),

            InvalidCredentialsException => (
                HttpStatusCode.Unauthorized,
                "No autorizado",
                exception.Message),

            _ => (
                HttpStatusCode.InternalServerError,
                "Error interno del servidor",
                "Ocurrió un error inesperado. Contacta al administrador si el problema persiste.")
        };

        httpContext.Response.StatusCode = (int)statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path,
            Type = $"https://httpstatuses.com/{(int)statusCode}"
        };

        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; // ya se manejó — no sigas buscando otro handler
    }
}