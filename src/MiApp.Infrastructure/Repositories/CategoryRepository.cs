using MiApp.Domain.Entities;
using MiApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using MiApp.Infrastructure.Data;

namespace MiApp.Infrastructure.Repositories;


public class CategoryRepository : ICategoryRepository
{

    private readonly AppDbContext _context;

    //inyeccion de dependencias: el repositorio recibe el dbcontext
    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }


    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories.ToListAsync();
    }


    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories.FindAsync(id);
    }

    public async Task<Category> CreateAsync(Category category)
    {
        // Agregamos la entidad al contexto
        await _context.Categories.AddAsync(category);

        // Guardamos cambios en la base de datos (Ejecuta INSERT)
        await _context.SaveChangesAsync();

        // Retornamos el valor, aqui el ID se genera automáticamente en la base de datos
        return category;
    }

    public async Task<Category?> UpdateAsync(Category category)
    {
        //Buscar la entidad existente en la base de datos
        var existing = await _context.Categories.FindAsync(category.Id);

        if(existing == null) return null;

        //Actualizar los campos permitidos
        existing.Name = category.Name;
        existing.Description = category.Description;
        existing.UpdatedAt = category.UpdatedAt;

        // Guardar los cambios (Ejecuta UPDATE)
        await _context.SaveChangesAsync();

        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        // Buscar la entidad en la base de datos
        var category = await _context.Categories.FindAsync(id);
        if(category == null) return false;

        // Marcar la entidad para eliminacion
        _context.Categories.Remove(category);

        // Guarda cambios (Ejecuta DELETE)
        await _context.SaveChangesAsync();

        return true;
    }


}