using Microsoft.AspNetCore.Mvc;
using MiApp.Application.Dtos;
using MiApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization; // ⬅️ Importante: agregar este using

namespace MiApp.API.Controllers;


[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    //Inyeccion de dependencias por constructor
    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }


    // GET: api/categories
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(categories);
    }

     // GET: api/categories/{id}
     [HttpGet("{id}")]
     [AllowAnonymous]
     public async Task<IActionResult> GetById(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if(category == null) return NotFound($"No se encontro la categoria con el ID {id}");
        return Ok(category);
    }

    // POST: api/categories
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto createDto)
    {
        if (!ModelState.IsValid)  return BadRequest(ModelState);

  
            var created = await _categoryService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id}, created);
        
    }

    // PUT: api/categories/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto updateDto)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var updated = await _categoryService.UpdateAsync(id, updateDto);
            if(updated == null) return NotFound($"No se encontro la categoria con el ID {id}");
            return Ok(updated);
        }
        catch(ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // DELETE: api/categories/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _categoryService.DeleteAsync(id);
        if (!deleted) return NotFound($"No se encontro la categoria con el ID {id}");

        return NoContent();
    }

}