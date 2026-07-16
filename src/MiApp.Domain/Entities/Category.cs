namespace MiApp.Domain.Entities;


// La clase Category representa una categoria de productos en el negocio.
// Es una "entidad de dominio" pura, sin anotaciones de base de datos
public class Category
{
    
    // Propiedad de identidad única. Sera el Id en la base de datos.
    public int Id {get; set;}

    // Nombre de la categoría. Es obligatorio, por eso no tiene '?'.
    public string Name {get; set;} = string.Empty;

    // Descripción de la categoría. Es opcional, por eso tiene '?'.
    public string? Description {get; set;}

    // Indica si la categoría está activa (para el "soft delete").
    // Por defecto, al crearse está activa.
    public bool IsActive {get; set;} = true;

    //Fecha de creación. Se asigna automáticamente con la hora UTC.
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;

    //Fecha de ultima Actualización. Es opcional (nullable) por que al crearse no se ha actualizado.
    public DateTime? UpdatedAt {get; set;}
}