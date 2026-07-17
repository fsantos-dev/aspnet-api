using MiApp.Application.Interfaces;
using MiApp.Application.Services;
using MiApp.Domain.Interfaces;
using MiApp.Infrastructure.Repositories;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Agregar controladores
builder.Services.AddControllers();

// 2. Configurar Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// 3. Registrar Dependencias (Inyección de dependencias)
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();


var app = builder.Build();


if (app.Environment.IsDevelopment())
{
      app.MapOpenApi();
    app.MapScalarApiReference(); // mapea /scalar/v1
    // app.UseSwagger();
    // app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


app.Run();
