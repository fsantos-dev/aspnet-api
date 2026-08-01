using MiApp.API.Middleware;

namespace MiApp.API.Middleware;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseCustomPipeline(this IApplicationBuilder app)
    {
        app.UseMiddleware<LoggingMiddleware>();
        return app;
    }
}