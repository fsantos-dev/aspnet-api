using MiApp.Application.Interfaces;
using MiApp.Application.Services;
using MiApp.Domain.Interfaces;
using MiApp.Infrastructure.Repositories;
using MiApp.Infrastructure.Data;
using Scalar.AspNetCore;
using Microsoft.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MiApp.API.Middleware;
using Microsoft.AspNetCore.Mvc;
using MiApp.Application.Mappings;

//Esto es para hacer un commit de prueba y probar pull request

var builder = WebApplication.CreateBuilder(args);

// 1. Agregar controladores
builder.Services.AddControllers();

// 2. Configurar Servicios
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

var jwtConfig = builder.Configuration.GetSection("Jwt");
var secretKey = jwtConfig["SecretKey"];
if (string.IsNullOrWhiteSpace(secretKey))
{
    throw new InvalidOperationException("La configuración Jwt:SecretKey no existe.");
}
var issuer = jwtConfig["Issuer"];
var audience = jwtConfig["Audience"];
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))

        
    };

    options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse(); // evita que el middleware default escriba su propia respuesta
                
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var problem = new ProblemDetails
                {
                    Title = "No autorizado",
                    Status = StatusCodes.Status401Unauthorized,
                    Detail = "No se proporcionó un token válido",
                    Instance = context.Request.Path,
                    Type = $"https://httpstatuses.com/{(int)StatusCodes.Status401Unauthorized}"
                };


                await context.Response.WriteAsJsonAsync(problem);
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                var problem = new ProblemDetails
                {
                    Title = "Prohibido",
                    Status = StatusCodes.Status403Forbidden,
                    Detail = "No tienes permisos para realizar esta acción",
                    Instance = context.Request.Path,
                    Type = $"https://httpstatuses.com/{(int)StatusCodes.Status403Forbidden}"
                };

                await context.Response.WriteAsJsonAsync(problem);
            }
        };
});
builder.Services.AddAuthorization();

// 3. Registrar Dependencias (Inyección de dependencias)

// 3.1 Registrar el DbContext con la cadena de conexión
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

// 3.2 Registrar los repositorios(Infrastructure)
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// 3.3 Registrar los servicios (Application)
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// 3.4 Configurar AutoMapper (escanea los mappings/perfiles en la capa de aplicacion)
builder.Services.AddAutoMapper(cfg => { }, typeof(UserProfile).Assembly);


// 4. Agregar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
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

app.UseExceptionHandler();
app.UseCustomPipeline();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


app.Run();
