using DroneBuilder.Application.Features.Products;
using DroneBuilder.Application.Features.Products.SetProductAttributes;
using FluentValidation.Results;

namespace DroneBuilder.Application.Tests.Features.Products;

public class SetProductAttributesCommandValidatorTests
{
    private readonly SetProductAttributesCommandValidator _validator = new();

    private static readonly Guid ProductId = Guid.NewGuid();

    [Fact]
    public void Validate_WhenAttributesAreValid_ShouldPass()
    {
        // Arrange
        var command = new SetProductAttributesCommand(ProductId,
            [new ProductAttributeModel("Wheelbase", "226 mm"), new ProductAttributeModel("Material", "Carbon")]);

        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenNamesDifferOnlyByCaseOrSpaces_ShouldFail()
    {
        // Arrange
        var command = new SetProductAttributesCommand(ProductId,
            [new ProductAttributeModel("Weight", "32 g"), new ProductAttributeModel(" weight ", "33 g")]);

        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenNameOrValueIsEmpty_ShouldFail()
    {
        // Arrange
        var command = new SetProductAttributesCommand(ProductId,
            [new ProductAttributeModel("", "226 mm"), new ProductAttributeModel("Material", "")]);

        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void Validate_WhenMoreThanFiftyAttributes_ShouldFail()
    {
        // Arrange
        List<ProductAttributeModel> attributes =
            Enumerable.Range(0, 51).Select(i => new ProductAttributeModel($"Name {i}", "Value")).ToList();

        // Act
        ValidationResult result = _validator.Validate(new SetProductAttributesCommand(ProductId, attributes));

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenAttributesIsNull_ShouldFail()
    {
        // Act
        ValidationResult result = _validator.Validate(new SetProductAttributesCommand(ProductId, null!));

        // Assert
        Assert.False(result.IsValid);
    }
}
