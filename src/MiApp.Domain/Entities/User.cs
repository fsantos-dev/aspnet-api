namespace MiApp.Domain.Entities;


/// Representa un usuario de la aplicación.
/// Por ahora está en memoria, pero luego se migrará a SQL Server.

public  class User
{
    public int Id {get; set;}
    public string Email {get; set;} = string.Empty;
    public string  PasswordHash { get; set; } = string.Empty; // Guardamos el Hash, no la contrasena en texto plano

    public string? FullName { get; set; }

    public bool IsActive {get; set;} = true;

    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;

    public ICollection<Category> Categories {get; set;} = new List<Category>();

}