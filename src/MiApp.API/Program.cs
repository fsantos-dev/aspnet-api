using MiApp.Application.Interfaces;
using MiApp.Application.Services;
using MiApp.Domain.Interfaces;
using MiApp.Infrastructure.Repositories;
using Scalar.AspNetCore;
using Microsoft.OpenApi;


var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Registrar DbContext con la cadena de conexión
// builder.Services.AddDbContext<AppDbContext>(options =>
// {
//     options.UseSqlServer(
//         builder.Configuration.GetConnectionString("DefaultConnection"));
// });


// 1. Agregar controladores
builder.Services.AddControllers();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

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

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


app.Run();
