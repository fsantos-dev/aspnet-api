using MiApp.Application.Interfaces;
using MiApp.Application.Services;
using MiApp.Domain.Interfaces;
using MiApp.Infrastructure.Repositories;
using MiApp.Infrastructure.Data;
using Scalar.AspNetCore;
using Microsoft.OpenApi;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// 1. Agregar controladores
builder.Services.AddControllers();

// 2. Configurar Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Servers = new List<OpenApiServer>
        {
            new() { Url = "https://curvature-unblessed-elm.ngrok-free.dev" }
        };
        return Task.CompletedTask;
    });
});
builder.Services.AddSwaggerGen();


// 3. Registrar Dependencias (Inyección de dependencias)

// 3.1 Registrar el DbContext con la cadena de conexión
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

// 3.2 Registrar el repositorio (Infrastructure)
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// 3.3 Registrar el servicio (Application)
builder.Services.AddScoped<ICategoryService, CategoryService>();


// 4. Agregar CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // mapea /scalar/v1
    // app.UseSwagger();
    // app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


app.Run();
