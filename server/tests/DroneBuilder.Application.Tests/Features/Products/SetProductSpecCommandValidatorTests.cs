using DroneBuilder.Application.Features.Products;
using DroneBuilder.Application.Features.Products.SetProductSpec;
using DroneBuilder.Domain.Entities.Components;
using FluentValidation.Results;

namespace DroneBuilder.Application.Tests.Features.Products;

public class SetProductSpecCommandValidatorTests
{
    private readonly SetProductSpecCommandValidator _validator = new();

    private static readonly Guid ProductId = Guid.NewGuid();

    [Fact]
    public void Validate_WhenMotorSpecIsValid_ShouldPass()
    {
        // Arrange
        var command = new SetProductSpecCommand(ProductId,
            new MotorSpecModel("2207", 1950, MountPattern.M16x16, 6, 6, 45m, 5m));

        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenMaxCellsLessThanMinCells_ShouldFail()
    {
        // Arrange
        var command = new SetProductSpecCommand(ProductId,
            new EscSpecModel(MountPattern.M30_5x30_5, 6, 4, 55m, BatteryConnector.Xt60));

        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.EndsWith(nameof(EscSpecModel.MaxCells)));
    }

    [Fact]
    public void Validate_WhenFrameHasNoMountPatterns_ShouldFail()
    {
        // Arrange
        var command = new SetProductSpecCommand(ProductId, new FrameSpecModel(5m, [], [], 19));

        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void Validate_WhenEnumValueIsUndefined_ShouldFail()
    {
        // Arrange
        var command = new SetProductSpecCommand(ProductId, new AntennaSpecModel((RfConnector)42));

        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenSpecIsNull_ShouldFail()
    {
        // Arrange
        var command = new SetProductSpecCommand(ProductId, null!);

        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
    }
}
