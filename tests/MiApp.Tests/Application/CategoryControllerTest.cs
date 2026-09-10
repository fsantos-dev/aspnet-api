using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MiApp.API.Controllers;
using MiApp.Application.Dtos;
using MiApp.Application.Interfaces;

namespace MiApp.Tests.Controllers;

public class CategoriesControllerTests
{
    private readonly Mock<ICategoryService> _categoryServiceMock;

    private readonly CategoriesController _controller;

    public CategoriesControllerTests()
    {
        _categoryServiceMock = new Mock<ICategoryService>();

        _controller = new CategoriesController(
            _categoryServiceMock.Object
        );
    }


    // ============================================================
    // GET ALL
    // ============================================================

    [Fact]
    public async Task GetAll_ShouldReturnOk_WhenCategoriesExist()
    {
        // Arrange

        var categories = new List<CategoryDto>
        {
            new CategoryDto
            {
                Id = 1,
                Name = "Comida",
                Description = "Gastos de comida"
            },
            new CategoryDto
            {
                Id = 2,
                Name = "Transporte",
                Description = "Gastos de transporte"
            }
        };

        _categoryServiceMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(categories);

        // Act

        var result = await _controller.GetAll();

        // Assert

        // ¿El controller devuelve 200 OK?
        var okResult = result.Result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        // ¿El resultado contiene las categorías?
        okResult.Value.Should().BeEquivalentTo(categories);

        // ¿Se llamó al service?
        _categoryServiceMock.Verify(
            x => x.GetAllAsync(),
            Times.Once
        );
    }


    // ============================================================
    // GET BY ID
    // ============================================================

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenCategoryExists()
    {
        // Arrange

        var id = 1;

        var category = new CategoryDto
        {
            Id = id,
            Name = "Comida",
            Description = "Gastos de comida"
        };

        _categoryServiceMock
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(category);

        // Act

        var result = await _controller.GetById(id);

        // Assert

        // ¿El controller devuelve 200 OK?
        var okResult = result.Result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        // ¿Devuelve la categoría correcta?
        okResult.Value.Should().BeEquivalentTo(category);

        // ¿Se llamó al service con el ID correcto?
        _categoryServiceMock.Verify(
            x => x.GetByIdAsync(id),
            Times.Once
        );
    }


    // ============================================================
    // CREATE
    // ============================================================

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction_WhenCategoryIsCreated()
    {
        // Arrange

        var createDto = new CreateCategoryDto
        {
            Name = "Comida",
            Description = "Gastos de comida"
        };

        var createdCategory = new CategoryDto
        {
            Id = 1,
            Name = createDto.Name,
            Description = createDto.Description
        };

        _categoryServiceMock
            .Setup(x => x.CreateAsync(createDto))
            .ReturnsAsync(createdCategory);

        // Act

        var result = await _controller.Create(createDto);

        // Assert

        // ¿El controller devuelve 201 Created?
        var createdResult = result.Result.Should()
            .BeOfType<CreatedAtActionResult>()
            .Subject;

        // ¿CreatedAtAction apunta a GetById?
        createdResult.ActionName.Should().Be(nameof(_controller.GetById));

        // ¿El ID de la ruta es correcto?
        createdResult.RouteValues.Should().ContainKey("id");
        createdResult.RouteValues!["id"].Should().Be(createdCategory.Id);

        // ¿Devuelve la categoría creada?
        createdResult.Value.Should().BeEquivalentTo(createdCategory);

        // ¿Se llamó al service con el DTO correcto?
        _categoryServiceMock.Verify(
            x => x.CreateAsync(createDto),
            Times.Once
        );
    }


    // ============================================================
    // UPDATE
    // ============================================================

    [Fact]
    public async Task Update_ShouldReturnOk_WhenCategoryIsUpdated()
    {
        // Arrange

        var id = 1;

        var updateDto = new UpdateCategoryDto
        {
            Name = "Hogar",
            Description = "Productos para el hogar"
        };

        var updatedCategory = new CategoryDto
        {
            Id = id,
            Name = updateDto.Name,
            Description = updateDto.Description
        };

        _categoryServiceMock
            .Setup(x => x.UpdateAsync(id, updateDto))
            .ReturnsAsync(updatedCategory);

        // Act

        var result = await _controller.Update(id, updateDto);

        // Assert

        // ¿El controller devuelve 200 OK?
        var okResult = result.Result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        // ¿Devuelve la categoría actualizada?
        okResult.Value.Should().BeEquivalentTo(updatedCategory);

        // ¿Se llamó al service con los parámetros correctos?
        _categoryServiceMock.Verify(
            x => x.UpdateAsync(id, updateDto),
            Times.Once
        );
    }


    // ============================================================
    // DELETE
    // ============================================================

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenCategoryIsDeleted()
    {
        // Arrange

        var id = 1;

        _categoryServiceMock
            .Setup(x => x.DeleteAsync(id))
            .Returns(Task.CompletedTask);

        // Act

        var result = await _controller.Delete(id);

        // Assert

        // ¿El controller devuelve 204 No Content?
        result.Result.Should()
            .BeOfType<NoContentResult>();

        // ¿Se llamó al service con el ID correcto?
        _categoryServiceMock.Verify(
            x => x.DeleteAsync(id),
            Times.Once
        );
    }
}