using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Features.Builds.CheckBuild;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Entities.Components;
using FluentResults;
using FluentValidation.Results;
using NSubstitute;

namespace DroneBuilder.Application.Tests.Features.Builds;

public class CheckBuildQueryHandlerTests
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();

    [Fact]
    public async Task ExecuteAsync_ShouldSumPriceByQuantityAndFailValidityOnErrors()
    {
        // Arrange
        var motor = new Product
        {
            Name = "Motor",
            Category = ProductCategory.Motor,
            Price = 20m,
            WeightGrams = 32m,
            Spec = new MotorSpec { StatorSize = "2207", Kv = 1950, MountPattern = MountPattern.M16x16, MinCells = 4, MaxCells = 6 }
        };
        var goggles = new Product { Name = "Goggles", Category = ProductCategory.Goggles, Price = 300m };
        _productRepository.GetProductsWithSpecsByIdsAsync(Arg.Any<ICollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([motor, goggles]);

        var handler = new CheckBuildQueryHandler(_productRepository);

        // Act
        Result<BuildCheckModel> result = await handler.ExecuteAsync(
            new CheckBuildQuery([new BuildItemModel(motor.Id, 4), new BuildItemModel(goggles.Id, 1)]),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(380m, result.Value.TotalPrice);
        Assert.False(result.Value.IsValid);
        Assert.Equal(128m, result.Value.Weight.DryGrams);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProductIsUnknown_ShouldReturnNotFound()
    {
        // Arrange
        _productRepository.GetProductsWithSpecsByIdsAsync(Arg.Any<ICollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var handler = new CheckBuildQueryHandler(_productRepository);

        // Act
        Result<BuildCheckModel> result = await handler.ExecuteAsync(
            new CheckBuildQuery([new BuildItemModel(Guid.NewGuid(), 1)]), CancellationToken.None);

        // Assert
        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public void Validator_WhenProductRepeatsOrQuantityIsZero_ShouldFail()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        var validator = new CheckBuildQueryValidator();

        // Act
        ValidationResult duplicates = validator.Validate(new CheckBuildQuery([new(id, 1), new(id, 2)]));
        ValidationResult zero = validator.Validate(new CheckBuildQuery([new(id, 0)]));
        ValidationResult empty = validator.Validate(new CheckBuildQuery([]));

        // Assert
        Assert.False(duplicates.IsValid);
        Assert.False(zero.IsValid);
        Assert.False(empty.IsValid);
    }
}
