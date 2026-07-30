using Microsoft.EntityFrameworkCore;
using MiApp.Domain.Entities;

namespace MiApp.Infrastructure.Data;


public class AppDbContext : DbContext
{
    //Constructor que recibe las opciones(Configuración de conexión)
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {
        
    }
}