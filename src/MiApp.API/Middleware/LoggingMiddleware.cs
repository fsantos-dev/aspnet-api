using System.Diagnostics;

namespace MiApp.API.Middleware;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    private const string Cyan = "\x1b[36m";
    private const string Magenta = "\x1b[35m";
    private const string Reset = "\x1b[0m";

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;

        // Leer el body del REQUEST (lo que envía el cliente)
        request.EnableBuffering();
        var requestBody = "";
        if (request.ContentLength > 0)
        {
            using var reader = new StreamReader(request.Body, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            request.Body.Position = 0; // resetear para que el resto del pipeline lo pueda leer
        }

        _logger.LogInformation("→ Inicia {Method} {Path}{QueryString} | Request: {Cyan}{RequestBody}{Reset}",
            request.Method, request.Path, request.QueryString, Cyan, requestBody, Reset);

        // Interceptar el body del RESPONSE para poder leerlo después
        var originalResponseBody = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "✗ Excepción no controlada en {Method} {Path}",
                request.Method, request.Path);
            throw; // la relanzamos para que el middleware de manejo de errores responda al cliente
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;

            // Leer lo que se generó como respuesta
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(responseBodyStream).ReadToEndAsync();
            responseBodyStream.Seek(0, SeekOrigin.Begin);

            // Log level distinto según el resultado, para poder filtrar fácil después
            var logLevel = statusCode >= 500 ? LogLevel.Error
                          : statusCode >= 400 ? LogLevel.Warning
                          : LogLevel.Information;

            _logger.Log(logLevel, "← Termina {Method} {Path} → {StatusCode} en {ElapsedMs}ms | Response: {Magenta}{ResponseBody}{Reset}",
                request.Method, request.Path, statusCode, stopwatch.ElapsedMilliseconds, Magenta, responseText, Reset);

            // copiar la respuesta de vuelta al stream original para que el cliente la reciba
            await responseBodyStream.CopyToAsync(originalResponseBody);
            context.Response.Body = originalResponseBody;
        }
    }
}