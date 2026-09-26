using DroneBuilder.Application.Common.Pagination;
using DroneBuilder.Application.Features.Products;
using DroneBuilder.Application.Features.Products.GetProducts;
using FluentValidation.Results;

namespace DroneBuilder.Application.Tests.Features.Products;

public class GetProductsQueryValidatorTests
{
    private readonly GetProductsQueryValidator _validator = new();

    private ValidationResult Validate(ProductFilterModel filter)
        => _validator.Validate(new GetProductsQuery(new PaginationParams(1, 20), filter));

    [Fact]
    public void Validate_WhenSpecFiltersAreConsistent_ShouldPass()
    {
        // Act
        ValidationResult result = Validate(new ProductFilterModel
        {
            Cells = 6,
            KvMin = 1700,
            KvMax = 1950,
            CapacityMin = 1000,
            CapacityMax = 1500,
            PropSizeInch = 5
        });

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenRangesAreReversed_ShouldFailForEachRange()
    {
        // Act
        ValidationResult result = Validate(new ProductFilterModel
        {
            KvMin = 2000,
            KvMax = 1000,
            CapacityMin = 1500,
            CapacityMax = 1000
        });

        // Assert
        Assert.Equal(2, result.Errors.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    public void Validate_WhenCellsAreOutOfRange_ShouldFail(int cells)
    {
        // Act
        ValidationResult result = Validate(new ProductFilterModel { Cells = cells });

        // Assert
        Assert.False(result.IsValid);
    }
}
