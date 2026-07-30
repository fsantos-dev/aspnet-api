using Microsoft.EntityFrameworkCore;
using MiApp.Domain.Entities;

namespace MiApp.Infrastructure.Data;


public class AppDbContext : DbContext
{
    //Constructor que recibe las opciones(Configuración de conexión)
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {
        
    }

    //Dbset representa una tabla en la base de datos
    public DbSet<Category> categories {get; set;}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        //Método donde se puede configurar el mapeo de las entidades (ej: llaves, índices, relaciones).
        // Aquí podemos agregar configuraciones adicionales:
        // - Índices
        // - Restricciones (ej: unicidad)
        // - Relaciones (ej: Category 1:N Product)

        modelBuilder.Entity<Category>(entity =>
        {
            // Indicar que la tabla se llamará "Categories" (por defecto sería "Category")
            entity.ToTable("Categories");

            //Configurar la clave primaria
            entity.HasKey(e => e.Id);

            //Configurar que el Id sea generado por la base de datos (autoincrement)
            entity.Property(e => e.Id).UseIdentityColumn();

            //Configurar que Name sea obligatorio y tenga un maximo de 100 caracteres
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

            //Configurar que Description sea opcional y tenga un maximo de 500 caracteres
            entity.Property(e => e.Description).HasMaxLength(500);

            //Configurar que IsActive tiene un valor por defecto (true)
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            //Configurar que CreatedAt se asigna automáticamente con la fecha UTC
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        });

        // Por ahora, dejamos la configuración por defecto.
        base.OnModelCreating(modelBuilder);
    }


//  protected override void OnModelCreating(ModelBuilder modelBuilder)
//     {
//         // Configurar la tabla Categories
//         modelBuilder.Entity<Category>(entity =>
//         {
//             // Nombre de la tabla en SQL Server
//             entity.ToTable("Categories");

//             // Configurar la clave primaria
//             entity.HasKey(c => c.Id);

//             // Configurar propiedades
//             entity.Property(c => c.Id)
//                 .ValueGeneratedOnAdd() // Autoincrementable
//                 .UseIdentityColumn(); // Para SQL Server

//             entity.Property(c => c.Name)
//                 .IsRequired()
//                 .HasMaxLength(100);

//             entity.Property(c => c.Description)
//                 .HasMaxLength(500);

//             entity.Property(c => c.CreatedAt)
//                 .IsRequired();

//             entity.Property(c => c.UpdatedAt)
//                 .IsRequired(false); // Puede ser NULL
//         });

//         base.OnModelCreating(modelBuilder);
//     }
}