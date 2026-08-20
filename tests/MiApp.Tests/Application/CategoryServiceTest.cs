using Moq;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;

using MiApp.Application.Dtos;
using MiApp.Application.Interfaces;
using MiApp.Application.Services;
using MiApp.Domain.Entities;
using MiApp.Domain.Interfaces;

namespace MiApp.Tests.Application;

public class CategoryServiceTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ICategoryRepository> _repositoryMock;
    private readonly Mock<IValidator<CreateCategoryDto>> _createValidatorMock;
    private readonly Mock<IValidator<UpdateCategoryDto>> _updateValidatorMock;

    private readonly CategoryService _service;

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


    [Fact]
    public async Task CreateAsync_ShouldCreateCategoryAndReturnDto()
    {
        // Arrange
        var createDto = new CreateCategoryDto
        {
            Name = "Electronicos",
            Description = "Gadgets y tecnologia",
        };

        _createValidatorMock
      .Setup(x => x.ValidateAsync(
          It.IsAny<CreateCategoryDto>(),
          It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult());

        //Simulamos que el repositorio guarda la categoria y devuelve una con Id asignado
        var expectedCategory = new Category(createDto.Name, createDto.Description, 5)
        {
            Id = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _repositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<Category>())).ReturnsAsync(expectedCategory);

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be(createDto.Name);
        result.Description.Should().Be(createDto.Description);
        result.IsActive.Should().BeTrue();

        // Verificar que se llamó al repositorio exactamente una vez
        _repositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<Category>()), Times.Once);

    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenNameIsEmpty()
    {
        // Arrange
        var createDto = new CreateCategoryDto
        {
            Name = "",
            Description = "Descripción"
        };

        _createValidatorMock
               .Setup(x => x.ValidateAsync(
                   It.IsAny<CreateCategoryDto>(),
                   It.IsAny<CancellationToken>()))
               .ReturnsAsync(new ValidationResult(
                   new[]
                   {
                new ValidationFailure(
                    "Name",
                    "El nombre es obligatorio"
                )
                   }
               ));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateAsync(createDto)
        );

        _repositoryMock.Verify(
            repo => repo.CreateAsync(It.IsAny<Category>()),
            Times.Never
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenNameExceedsMaximumLength()
    {
        // Arrange
        var createDto = new CreateCategoryDto
        {
            Name = new string('A', 101),
            Description = "Descripción"
        };

        _createValidatorMock
            .Setup(x => x.ValidateAsync(
                It.IsAny<CreateCategoryDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
                new[]
                {
                new ValidationFailure(
                    "Name",
                    "El nombre no puede superar los 100 caracteres"
                )
                }
            ));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateAsync(createDto)
        );

        _repositoryMock.Verify(
            repo => repo.CreateAsync(It.IsAny<Category>()),
            Times.Never
        );
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCategoryAndReturnDto()
    {

        // ARRANGE
        var id = 1;

        var updateDto = new UpdateCategoryDto
        {
            Name = "Jardin",
            Description = "Productos de jardin"
        };

        var existingCategory = new Category(
            "Electonicos",
            "Gadgets y Tecnologia",
            5
        )
        {
            Id = id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        _updateValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<UpdateCategoryDto>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());

        _repositoryMock.Setup(repository => repository.GetByIdAsync(id, 5)).ReturnsAsync(existingCategory);


        //ACT

        var result = await _service.UpdateAsync(id, updateDto);


        //ASERT
        result.Should().NotBeNull();
        result.Id.Should().Be(id);
        result.Name.Should().Be(updateDto.Name);
        result.Description.Should().Be(updateDto.Description);

        _repositoryMock.Verify(repo => repo.GetByIdAsync(id, 5), Times.Once);
        _repositoryMock.Verify(repo => repo.UpdateAsync(existingCategory), Times.Once);

    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteCategory()
    {
        // ARRANGE
        var id = 1;

        var existingCategory = new Category(
           "Electonicos",
           "Gadgets y Tecnologia",
           5
       )
        {
            Id = id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        _repositoryMock.Setup(repository => repository.GetByIdAsync(id, 5)).ReturnsAsync(existingCategory);

        //Act
        await _service.DeleteAsync(id);

        _repositoryMock.Verify(repo => repo.GetByIdAsync(id, 5), Times.Once);
        _repositoryMock.Verify(repo => repo.DeleteAsync(id), Times.Once);

    }


    [Fact]
    public async Task DeleteAsync_ShouldThrowKeyNotFoundException_WhenCategoryDoesNotExist()
    {
        // Arrange
        var id = 1;

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(id, 5))
            .ReturnsAsync((Category?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.DeleteAsync(id)
        );

        _repositoryMock.Verify(
            repo => repo.DeleteAsync(id),
            Times.Never
        );
    }

    [Fact]
    public async Task GetAll_ShouldGetAllCategoryAndReturnDto()
    {
        //Arrange

        var categories = new List<Category>
    {
        new Category("Electronicos", "Gadgets", 5)
        {
            Id = 1,
            IsActive = true
        },

        new Category("Jardin", "Productos de jardin", 5)
        {
            Id = 2,
            IsActive = true
        }
    };
        _repositoryMock
       .Setup(repo => repo.GetAllAsync(5))
       .ReturnsAsync(categories);

        //Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        result.Should().Contain(x =>
            x.Id == 1 &&
            x.Name == "Electronicos"
        );

        result.Should().Contain(x =>
            x.Id == 2 &&
            x.Name == "Jardin"
        );

        _repositoryMock.Verify(
            repo => repo.GetAllAsync(5),
            Times.Once
        );
    }

    [Fact]
    public async Task GetById_ShouldGetByIdCategoryAndReturnDto()
    {
        //Arrange

        var id = 1;

        var existingCategory = new Category(
            "Electonicos",
            "Gadgets y Tecnologia",
            5
        )
        {
            Id = id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(id, 5)).ReturnsAsync(existingCategory);

        //Act
        var result = await _service.GetByIdAsync(id);

        result.Should().NotBeNull();
        result.Id.Should().Be(id);
        result.Name.Should().Be(existingCategory.Name);
        result.Description.Should().Be(existingCategory.Description);
        result.IsActive.Should().BeTrue();

        _repositoryMock.Verify(
        repo => repo.GetByIdAsync(id, 5),
        Times.Once
    );

    }

}