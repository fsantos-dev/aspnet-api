using Moq;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;

using MiApp.Application.Dtos;
using MiApp.Application.Interfaces;
using MiApp.Application.Services;
using MiApp.Domain.Entities;
using MiApp.Domain.Interfaces;
using FluentAssertions.Equivalency.Steps;

namespace MiApp.Tests.Application;

public class CategoryServiceTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ICategoryRepository> _repositoryMock;
    private readonly Mock<IValidator<CreateCategoryDto>> _createValidatorMock;
    private readonly Mock<IValidator<UpdateCategoryDto>> _updateValidatorMock;

    private readonly CategoryService _service;


    /*
        IMPORTANTE a la hora de hacer tests identificar que pruebas necesita un metodo
        1. Happy path
        2. Errores / excepciones
        3. Decisiones (if, switch...)
        4. Casos límite
        5. Dependencias importantes
        6. Reglas de negocio
    */


    /* AAA pattern.
    Arrange (Preparar) : Aquí dejamos todo listo para ejecutar aquello que queremos probar.
        -Creamos/configuramos los mocks.
        -Preparamos los datos de entrada.
        -Configuramos qué deben devolver los mocks.
        -Creamos la instancia de la clase que estamos probando.
    
    Act (Ejecutar): Aquí hacemos una sola acción principal:
        -Ejecuta el comportamiento que quiero probar.

    Assert (Comprobar) : Aquí verificamos que el resultado sea el esperado 
        -también podemos verificar que el servicio haya interactuado correctamente con una dependencia
    */

    /*
        Para nuestras aserciones tenemos 2 formas.
        Asserts de xunit:
            -Assert.notNull(...)
        FluentAssertions:
            -Should().NotBeNull()
    */
    public CategoryServiceTests()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _repositoryMock = new Mock<ICategoryRepository>();
        _createValidatorMock = new Mock<IValidator<CreateCategoryDto>>();
        _updateValidatorMock = new Mock<IValidator<UpdateCategoryDto>>();

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(5);

        _service = new CategoryService(
            _currentUserServiceMock.Object,
            _repositoryMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object
        );
    }

    // ============================================================
    // GetAllAsync
    // ============================================================

    [Fact]
    public async Task GetAllAsync_ShouldReturnCategoriesForCurrentUser()
    {
        // Arrange
        var categories = new List<Category>
        {
            new Category("Comida", "Productos de comida", 5),
            new Category("Cocina", "Productos de cocina", 5)
        };
        _repositoryMock.Setup(rep => rep.GetAllAsync(5)).ReturnsAsync(categories);
        // Act
        var result = await _service.GetAllAsync();
        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Name == "Comida" && c.Description == "Productos de comida");
        result.Should().Contain(c => c.Name == "Cocina" && c.Description == "Productos de cocina");
        _repositoryMock.Verify(rep => rep.GetAllAsync(5), Times.Once);

    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyCategoriesForCurrentUser()
    {
        // Arrange
        var categories = new List<Category>();
        _repositoryMock.Setup(rep => rep.GetAllAsync(5)).ReturnsAsync(categories);
        // Act
        var result = await _service.GetAllAsync();
        // Assert
        result.Should().BeEmpty();
        _repositoryMock.Verify(rep => rep.GetAllAsync(5), Times.Once);
    }

    // ============================================================
    //  GetByIdAsync
    // ============================================================

    [Fact]
    public async Task GetByIdAsync_shouldReturnCategory_ForCurrentUser()
    {
        // Arrange
        var id = 1;
        var category = new Category("Jardin", "Productos de jardin", 5);
        _repositoryMock.Setup(rep => rep.GetByIdAsync(id, 5)).ReturnsAsync(category);
        // Act
        var result = await _service.GetByIdAsync(id);
        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Jardin");
        result.Description.Should().Be("Productos de jardin");
        _repositoryMock.Verify(rep => rep.GetByIdAsync(id, 5), Times.Once);

    }


    [Fact]
    public async Task GetByIdAsync_shouldReturnNullCategory_ForCurrentUser()
    {
        // Arrange
        var id = 1;
        _repositoryMock.Setup(rep => rep.GetByIdAsync(id, 5)).ReturnsAsync((Category?)null);
        // Act
        //Cuando un metodo no devuelve nada y debe lanzar una excepcion 
        Func<Task> act = () => _service.GetByIdAsync(1);
        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
        _repositoryMock.Verify(rep => rep.GetByIdAsync(id, 5), Times.Once);

    }


    // ============================================================
    //  GetByIdAsync
    // ============================================================
    [Fact]
    public async Task CreateAsync_ShouldCreateCategoryAndReturnDto()
    {
        // Arrange
        //el dto que recibimos
        var createDto = new CreateCategoryDto
        {
            Name = "Cocina",
            Description = "Productos de cocina",
        };

        var validationResult = new ValidationResult();
        _createValidatorMock.Setup(x => x.ValidateAsync(createDto, default)).ReturnsAsync(validationResult);

        var createdCategory = new Category(createDto.Name, createDto.Description, 5);
        _repositoryMock.Setup(x => x.CreateAsync(It.IsAny<Category>())).ReturnsAsync(createdCategory);

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        //validacion sobre lo que retorna el servicio
        result.Should().NotBeNull();
        result.Name.Should().Be("Cocina");
        result.Description.Should().Be("Productos de cocina");

        //validacion sobre lo que recibe el repositorio
        _repositoryMock.Verify(x => x.CreateAsync(
            It.Is<Category>(c => c.Name == createDto.Name
            && c.Description == createDto.Description
            && c.UserId == 5)
        ), Times.Once);

    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException()
    {
        // Arrange
        var createDto = new CreateCategoryDto
        {
            Name = "",
            Description = "Productos de tecnologia"
        };

        var validationResult = new ValidationResult(
            new[]{
                new ValidationFailure("Name", "El nombre es obligatorio"),
                new ValidationFailure("Description", "La descripcion es obligatoria")
            }
        );
        _createValidatorMock.Setup(x => x.ValidateAsync(createDto, default)).ReturnsAsync(validationResult);

        // Act 
        Func<Task> act = () => _service.CreateAsync(createDto);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        _repositoryMock.Verify(x => x.CreateAsync(It.IsAny<Category>()), Times.Never);
    }


    [Fact]
    public async Task UpdateAsync_ShouldUpdateCategoryAndReturnDto()
    {
        // Arrange
        var id = 1;
        var updateDto = new UpdateCategoryDto
        {
            Name = "Hogar",
            Description = "Productos para el hogar"
        };
        var existingCategory = new Category("Hogar", "Productos para el hogar", 5)
        {
            Id = 1
        };

        var validationResult = new ValidationResult();
        _updateValidatorMock.Setup(x => x.ValidateAsync(updateDto, default)).ReturnsAsync(validationResult);

        _repositoryMock.Setup(x => x.GetByIdAsync(id, 5)).ReturnsAsync(existingCategory);

        // Act
        var result = await _service.UpdateAsync(id, updateDto);

        // Assets
        result.Should().NotBeNull();
        result.Name.Should().Be("Hogar");
        result.Description.Should().Be("Productos para el hogar");

        _repositoryMock.Verify(x => x.GetByIdAsync(id, 5), Times.Once);
        _repositoryMock.Verify(x => x.UpdateAsync(It.Is<Category>(c =>
            c.Id == id &&
            c.Name == updateDto.Name &&
            c.Description == updateDto.Description &&
            c.UserId == 5
        )), Times.Once);

    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowValidationException()
    {

        // Arrange
        var id = 1;
        var updateDto = new UpdateCategoryDto
        {
            Name = "",
            Description = ""
        };

        var validationResult = new ValidationResult(
             new[]{
                new ValidationFailure("Name", "El nombre es obligatorio"),
                new ValidationFailure("Description", "La descripcion es obligatoria")
            }
        );
        _updateValidatorMock.Setup(x => x.ValidateAsync(updateDto, default)).ReturnsAsync(validationResult);

        // Act
        Func<Task> act = () => _service.UpdateAsync(id, updateDto);

        //Assets
        await act.Should().ThrowAsync<ValidationException>();
        _repositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>()),
            Times.Never
        );

        _repositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Category>()),
            Times.Never
        );

    }

    [Fact]
    public async Task UpdateAsync_shouldReturnNullCategory()
    {

        // Arrange
        var id = 1;
        var updateDto = new UpdateCategoryDto
        {
            Name = "Tecnologia",
            Description = "Productos de tecnologia"
        };

        var validationResult = new ValidationResult();
        _updateValidatorMock.Setup(x => x.ValidateAsync(updateDto, default)).ReturnsAsync(validationResult);


        _repositoryMock.Setup(x => x.GetByIdAsync(id, 5)).ReturnsAsync((Category?)null);

        // Act
        Func<Task> act = () => _service.UpdateAsync(id, updateDto);

        // Asserts
        await act.Should().ThrowAsync<KeyNotFoundException>();

        _repositoryMock.Verify(x => x.GetByIdAsync(id, 5), Times.Once);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Category>()), Times.Never);

    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNoContent()
    {   

        // Arrange
        var id = 1;
        var category = new Category("Tenologia", "Productos de tecnologia", 5);

        _repositoryMock.Setup(x => x.GetByIdAsync(id, 5)).ReturnsAsync(category);

        // Act
        await _service.DeleteAsync(id);

        // Asserts
        _repositoryMock.Verify(x => x.GetByIdAsync(id, 5), Times.Once);
        _repositoryMock.Verify(x => x.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNull()
    {

        // Arrange
        var id = 1;
        _repositoryMock.Setup(x => x.GetByIdAsync(id, 5)).ReturnsAsync((Category?)null);

        // Act
        Func<Task> act = () => _service.DeleteAsync(id);

        // Asserts
        await act.Should().ThrowAsync<KeyNotFoundException>();

        _repositoryMock.Verify(x => x.GetByIdAsync(id, 5), Times.Once);

        _repositoryMock.Verify(x => x.DeleteAsync(id), Times.Never);
    }




}