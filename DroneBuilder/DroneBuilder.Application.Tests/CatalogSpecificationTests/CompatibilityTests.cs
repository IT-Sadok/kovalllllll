using DroneBuilder.Application.Features.Catalog.Compatibility.CheckCompatibility;
using DroneBuilder.Application.Features.Catalog.Compatibility.Models;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;
using NSubstitute;

namespace DroneBuilder.Application.Tests.CatalogSpecificationTests;

public class CompatibilityTests
{
    private readonly ICompatibilityRepository _repository = Substitute.For<ICompatibilityRepository>();

    [Fact]
    public async Task CheckCompatibility_NormalizesUnitsAndPassesLessThanRule()
    {
        CompatibilityFixture fixture = CreateNumericFixture(30m, 40000m);
        ConfigureRepository(fixture);

        Result<CompatibilityCheckResultModel> result = await new CheckCompatibilityCommandHandler(_repository)
            .ExecuteCommandAsync(
                new CheckCompatibilityCommand(new CheckCompatibilityModel
                {
                    LeftVariantId = fixture.LeftVariant.Id,
                    RightVariantId = fixture.RightVariant.Id
                }),
                CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Compatible", result.Value.Status);
        Assert.True(result.Value.IsCompatible);
        Assert.Equal("Compatible", Assert.Single(result.Value.Rules).Status);
    }

    [Fact]
    public async Task CheckCompatibility_WhenNumericRuleFails_ReturnsIncompatible()
    {
        CompatibilityFixture fixture = CreateNumericFixture(50m, 40000m);
        ConfigureRepository(fixture);

        Result<CompatibilityCheckResultModel> result = await new CheckCompatibilityCommandHandler(_repository)
            .ExecuteCommandAsync(
                new CheckCompatibilityCommand(new CheckCompatibilityModel
                {
                    LeftVariantId = fixture.LeftVariant.Id,
                    RightVariantId = fixture.RightVariant.Id
                }),
                CancellationToken.None);

        Assert.Equal("Incompatible", result.Value.Status);
        Assert.False(result.Value.IsCompatible);
        Assert.Equal("Motor current exceeds ESC capacity.", Assert.Single(result.Value.Rules).Message);
    }

    [Fact]
    public async Task CheckCompatibility_WithNoApplicableRules_ReturnsUnknown()
    {
        CompatibilityFixture fixture = CreateNumericFixture(30m, 40000m);
        _repository.GetVariantAsync(fixture.LeftVariant.Id, Arg.Any<CancellationToken>())
            .Returns(fixture.LeftVariant);
        _repository.GetVariantAsync(fixture.RightVariant.Id, Arg.Any<CancellationToken>())
            .Returns(fixture.RightVariant);
        _repository.GetApplicableRulesAsync(
                Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns([]);

        Result<CompatibilityCheckResultModel> result = await new CheckCompatibilityCommandHandler(_repository)
            .ExecuteCommandAsync(
                new CheckCompatibilityCommand(new CheckCompatibilityModel
                {
                    LeftVariantId = fixture.LeftVariant.Id,
                    RightVariantId = fixture.RightVariant.Id
                }),
                CancellationToken.None);

        Assert.Equal("Unknown", result.Value.Status);
        Assert.Null(result.Value.IsCompatible);
    }

    [Fact]
    public void ValidateDefinition_WithDifferentNumericDimensions_Throws()
    {
        var ampere = new UnitDefinition { Dimension = "current" };
        var volt = new UnitDefinition { Dimension = "voltage" };
        Property leftProperty = CreateNumberProperty("max-current", ampere);
        Property rightProperty = CreateNumberProperty("max-voltage", volt);
        ComponentType leftType = CreateComponentType("motor", leftProperty);
        ComponentType rightType = CreateComponentType("esc", rightProperty);
        var rule = new CompatibilityRule
        {
            LeftComponentType = leftType,
            LeftComponentTypeId = leftType.Id,
            LeftProperty = leftProperty,
            LeftPropertyId = leftProperty.Id,
            RightComponentType = rightType,
            RightComponentTypeId = rightType.Id,
            RightProperty = rightProperty,
            RightPropertyId = rightProperty.Id,
            Operator = CompatibilityOperator.LeftLessThanOrEqualRight
        };

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(rule.ValidateDefinition);

        Assert.Contains("same dimension", exception.Message);
    }

    private void ConfigureRepository(CompatibilityFixture fixture)
    {
        _repository.GetVariantAsync(fixture.LeftVariant.Id, Arg.Any<CancellationToken>())
            .Returns(fixture.LeftVariant);
        _repository.GetVariantAsync(fixture.RightVariant.Id, Arg.Any<CancellationToken>())
            .Returns(fixture.RightVariant);
        _repository.GetApplicableRulesAsync(
                fixture.LeftType.Id,
                fixture.RightType.Id,
                Arg.Any<CancellationToken>())
            .Returns([fixture.Rule]);
    }

    private static CompatibilityFixture CreateNumericFixture(decimal motorCurrent, decimal escCurrent)
    {
        var ampere = new UnitDefinition
        {
            Code = "ampere",
            Dimension = "current",
            ConversionFactorToBase = 1m
        };
        var milliampere = new UnitDefinition
        {
            Code = "milliampere",
            Dimension = "current",
            ConversionFactorToBase = 0.001m
        };
        Property motorProperty = CreateNumberProperty("motor-max-current", ampere);
        Property escProperty = CreateNumberProperty("esc-current", milliampere);
        ComponentType motorType = CreateComponentType("motor", motorProperty);
        ComponentType escType = CreateComponentType("esc", escProperty);
        ProductVariant motorVariant = CreateVariant(motorType, motorProperty, motorCurrent);
        ProductVariant escVariant = CreateVariant(escType, escProperty, escCurrent);
        var rule = new CompatibilityRule
        {
            Code = "motor-current-within-esc",
            Name = "Motor current within ESC capacity",
            LeftComponentTypeId = motorType.Id,
            LeftComponentType = motorType,
            LeftPropertyId = motorProperty.Id,
            LeftProperty = motorProperty,
            RightComponentTypeId = escType.Id,
            RightComponentType = escType,
            RightPropertyId = escProperty.Id,
            RightProperty = escProperty,
            Operator = CompatibilityOperator.LeftLessThanOrEqualRight,
            FailureMessage = "Motor current exceeds ESC capacity."
        };
        return new CompatibilityFixture(motorType, escType, motorVariant, escVariant, rule);
    }

    private static ProductVariant CreateVariant(
        ComponentType componentType,
        Property property,
        decimal value)
    {
        var product = new Product
        {
            ComponentType = componentType,
            ComponentTypeId = componentType.Id
        };
        var variant = new ProductVariant
        {
            Product = product,
            ProductId = product.Id,
            IsDefault = true
        };
        variant.Specifications.Add(new ProductVariantPropertyValue
        {
            ProductVariant = variant,
            ProductVariantId = variant.Id,
            Property = property,
            PropertyId = property.Id,
            NumericValue = value
        });
        product.Variants.Add(variant);
        return variant;
    }

    private static Property CreateNumberProperty(string code, UnitDefinition unit)
    {
        return new Property
        {
            Code = code,
            Name = code,
            DataType = SpecificationDataType.Number,
            UnitDefinition = unit,
            UnitDefinitionId = unit.Id,
            IsCompatibilityRelevant = true
        };
    }

    private static ComponentType CreateComponentType(string code, Property property)
    {
        var type = new ComponentType { Code = code, Name = code };
        type.Properties.Add(new ComponentTypeProperty
        {
            ComponentTypeId = type.Id,
            PropertyId = property.Id,
            Property = property,
            IsVariantSpecific = true
        });
        return type;
    }

    private sealed record CompatibilityFixture(
        ComponentType LeftType,
        ComponentType RightType,
        ProductVariant LeftVariant,
        ProductVariant RightVariant,
        CompatibilityRule Rule);
}
