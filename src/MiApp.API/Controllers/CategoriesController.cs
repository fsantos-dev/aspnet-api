using Microsoft.AspNetCore.Mvc;
using MiApp.Application.Dtos;
using MiApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using MiApp.Domain.Entities;
using System.Security.Claims; // ⬅️ Importante: agregar este using

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
    public async Task<ActionResult<CategoryDto>> GetAll()
    {

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
            return Unauthorized();

        var userId = int.Parse(userIdClaim.Value);
        var categories = await _categoryService.GetAllAsync(userId);
        return Ok(categories);
    }

    // GET: api/categories/{id}
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        return Ok(category);
    }

    // POST: api/categories
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryDto createDto)
    {
        var created = await _categoryService.CreateAsync(createDto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/categories/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryDto>> Update(int id, UpdateCategoryDto updateDto)
    {
        var updated = await _categoryService.UpdateAsync(id, updateDto);
        return Ok(updated);
    }

    // DELETE: api/categories/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult<CategoryDto>> Delete(int id)
    {
        await _categoryService.DeleteAsync(id);
        return NoContent();
    }


}