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


    public async Task<IEnumerable<Category>> GetAllAsync(int userId)
    {
        return await _context.Categories.Where(c => c.UserId == userId).
        ToListAsync();
    }


    public async Task<Category?> GetByIdAsync(int id, int userId)
    {
        return await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
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

    public async Task UpdateAsync(Category category)
    {
        // Guardar los cambios (Ejecuta UPDATE)
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        // Buscar la entidad en la base de datos
        var category = await _context.Categories.FindAsync(id);
        // Marcar la entidad para eliminacion
        _context.Categories.Remove(category!);
        // Guarda cambios (Ejecuta DELETE)
        await _context.SaveChangesAsync();
    }


}