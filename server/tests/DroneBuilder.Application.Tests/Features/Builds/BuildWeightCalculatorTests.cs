using DroneBuilder.Application.Features.Builds.Compatibility;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Entities.Components;

namespace DroneBuilder.Application.Tests.Features.Builds;

public class BuildWeightCalculatorTests
{
    private static BuildPart Part(ProductCategory category, decimal? weight, int quantity = 1, ComponentSpec? spec = null)
        => new(Guid.NewGuid(), category.ToString(), category, spec, 10m, weight, quantity);

    [Fact]
    public void Calculate_ShouldCountMotorsByQuantityPropsPerMotorAndBatteryOnlyInTakeOffWeight()
    {
        // Arrange
        var build = new BuildParts([
            Part(ProductCategory.Frame, 118m),
            Part(ProductCategory.Motor, 32.9m, 4, new MotorSpec { MaxThrustGrams = 1700 }),
            Part(ProductCategory.Propeller, 4.4m, quantity: 2),
            Part(ProductCategory.Stack, 19m),
            Part(ProductCategory.Battery, 215m, quantity: 3),
            Part(ProductCategory.Goggles, 470m)
        ]);

        // Act
        BuildWeight weight = BuildWeightCalculator.Calculate(build);

        // Assert
        Assert.Equal(286.2m, weight.DryGrams);
        Assert.Equal(501.2m, weight.AllUpGrams);
        Assert.Equal(13.6m, weight.ThrustToWeight);
        Assert.True(weight.IsComplete);
        Assert.Empty(weight.MissingWeightProductIds);
    }

    [Fact]
    public void Calculate_WhenAWeightIsUnknown_ShouldLeaveTakeOffWeightOpen()
    {
        // Arrange
        BuildPart camera = Part(ProductCategory.Camera, null);
        var build = new BuildParts([Part(ProductCategory.Frame, 118m), camera, Part(ProductCategory.Battery, 215m)]);

        // Act
        BuildWeight weight = BuildWeightCalculator.Calculate(build);

        // Assert
        Assert.Equal(118m, weight.DryGrams);
        Assert.Null(weight.AllUpGrams);
        Assert.Null(weight.ThrustToWeight);
        Assert.False(weight.IsComplete);
        Assert.Equal([camera.ProductId], weight.MissingWeightProductIds);
    }

    [Fact]
    public void Calculate_WhenThereIsNoBattery_ShouldNotClaimATakeOffWeight()
    {
        // Act
        BuildWeight weight = BuildWeightCalculator.Calculate(new BuildParts([Part(ProductCategory.Frame, 118m)]));

        // Assert
        Assert.Null(weight.AllUpGrams);
        Assert.False(weight.IsComplete);
    }
}
