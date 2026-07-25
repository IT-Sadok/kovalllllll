using DroneBuilder.Application.Features.Catalog.ProductSpecifications;
using DroneBuilder.Application.Features.Catalog.ProductSpecifications.Models;
using FluentValidation.Results;

namespace DroneBuilder.Application.Tests.CatalogSpecificationTests;

public class SpecificationValueInputValidatorTests
{
    private readonly SpecificationValueInputValidator<UpdateProductSpecificationModel> _validator = new();

    [Fact]
    public async Task Validate_WithNoRepresentation_IsInvalid()
    {
        ValidationResult result = await _validator.ValidateAsync(new UpdateProductSpecificationModel());

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithMoreThanOneRepresentation_IsInvalid()
    {
        var model = new UpdateProductSpecificationModel
        {
            NumericValue = 30m,
            TextValue = "30A"
        };

        ValidationResult result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithFalseBooleanValue_IsValid()
    {
        ValidationResult result = await _validator.ValidateAsync(
            new UpdateProductSpecificationModel { BooleanValue = false });

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithIncompleteRange_IsInvalid()
    {
        ValidationResult result = await _validator.ValidateAsync(
            new UpdateProductSpecificationModel { MinNumericValue = 4m });

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithOrderedCompleteRange_IsValid()
    {
        ValidationResult result = await _validator.ValidateAsync(
            new UpdateProductSpecificationModel
            {
                MinNumericValue = 4m,
                MaxNumericValue = 6m
            });

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithReversedRange_IsInvalid()
    {
        ValidationResult result = await _validator.ValidateAsync(
            new UpdateProductSpecificationModel
            {
                MinNumericValue = 6m,
                MaxNumericValue = 4m
            });

        Assert.False(result.IsValid);
    }
}
