using DroneBuilder.Application.Features.Catalog.Metadata.Models;
using DroneBuilder.Application.Features.Catalog.Metadata.UpdateUnit;
using DroneBuilder.Application.Features.Catalog.Metadata.UpsertComponentTypeProperty;
using DroneBuilder.Application.Features.Catalog.Properties.CreateProperty;
using DroneBuilder.Application.Features.Catalog.Properties.DeleteProperty;
using DroneBuilder.Application.Features.Catalog.Values.CreateValue;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using NSubstitute;

namespace DroneBuilder.Application.Tests.CatalogSpecificationTests;

public class CatalogMetadataAdminTests
{
    [Fact]
    public async Task CreateCompatibilityNumberProperty_WithoutUnit_ReturnsValidationError()
    {
        IPropertyRepository repository = Substitute.For<IPropertyRepository>();
        repository.IsCodeInUseAsync("max-current", null, Arg.Any<CancellationToken>()).Returns(false);
        var model = new CreatePropertyModel
        {
            Code = "max-current",
            Name = "Max current",
            DataType = "Number",
            IsCompatibilityRelevant = true
        };

        Result<PropertyModel> result = await new CreatePropertyCommandHandler(repository)
            .ExecuteCommandAsync(new CreatePropertyCommand(model), CancellationToken.None);

        Assert.True(result.HasError<ValidationError>());
        await repository.DidNotReceive().AddPropertyAsync(Arg.Any<Property>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpsertComponentTypeProperty_AddsValidRule()
    {
        ICatalogMetadataRepository repository = Substitute.For<ICatalogMetadataRepository>();
        var componentType = new ComponentType { Code = "motor", Name = "Motor" };
        var property = new Property
        {
            Code = "motor-kv",
            Name = "Motor KV",
            DataType = SpecificationDataType.Number
        };
        repository.GetComponentTypeAsync(componentType.Id, Arg.Any<CancellationToken>())
            .Returns(componentType);
        repository.GetPropertyAsync(property.Id, Arg.Any<CancellationToken>()).Returns(property);
        var model = new UpsertComponentTypePropertyModel
        {
            PropertyId = property.Id,
            IsVariantSpecific = true,
            SortOrder = 2
        };

        Result<ComponentTypePropertyModel> result =
            await new UpsertComponentTypePropertyCommandHandler(repository)
                .ExecuteCommandAsync(
                    new UpsertComponentTypePropertyCommand(componentType.Id, property.Id, model),
                    CancellationToken.None);

        Assert.True(result.IsSuccess);
        ComponentTypeProperty rule = Assert.Single(componentType.Properties);
        Assert.True(rule.IsVariantSpecific);
        Assert.Equal(2, rule.SortOrder);
    }

    [Fact]
    public async Task UpsertRequiredRule_WhenPublishedProductHasNoValue_ReturnsConflict()
    {
        ICatalogMetadataRepository repository = Substitute.For<ICatalogMetadataRepository>();
        var componentType = new ComponentType { Code = "motor", Name = "Motor" };
        var property = new Property
        {
            Code = "motor-kv",
            Name = "Motor KV",
            DataType = SpecificationDataType.Number
        };
        var product = new Product
        {
            Name = "Motor",
            ComponentType = componentType,
            ComponentTypeId = componentType.Id
        };
        product.EnsureDefaultVariant();
        componentType.Products.Add(product);
        repository.GetComponentTypeAsync(componentType.Id, Arg.Any<CancellationToken>())
            .Returns(componentType);
        repository.GetPropertyAsync(property.Id, Arg.Any<CancellationToken>()).Returns(property);

        Result<ComponentTypePropertyModel> result =
            await new UpsertComponentTypePropertyCommandHandler(repository)
                .ExecuteCommandAsync(
                    new UpsertComponentTypePropertyCommand(
                        componentType.Id,
                        property.Id,
                        new UpsertComponentTypePropertyModel
                        {
                            PropertyId = property.Id,
                            IsRequired = true
                        }),
                    CancellationToken.None);

        Assert.True(result.HasError<ConflictError>());
        await repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateUsedUnit_ChangingConversionFactor_ReturnsConflict()
    {
        ICatalogMetadataRepository repository = Substitute.For<ICatalogMetadataRepository>();
        var unit = new UnitDefinition
        {
            Code = "ampere",
            Name = "Ampere",
            Symbol = "A",
            Dimension = "current",
            ConversionFactorToBase = 1m
        };
        unit.Properties.Add(new Property { Code = "current", Name = "Current" });
        repository.GetUnitAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        repository.IsUnitCodeInUseAsync(unit.Code, unit.Id, Arg.Any<CancellationToken>()).Returns(false);

        Result<UnitDefinitionModel> result = await new UpdateUnitCommandHandler(repository)
            .ExecuteCommandAsync(
                new UpdateUnitCommand(
                    unit.Id,
                    new UpdateUnitDefinitionModel { ConversionFactorToBase = 1000m }),
                CancellationToken.None);

        Assert.True(result.HasError<ConflictError>());
        Assert.Equal(1m, unit.ConversionFactorToBase);
    }

    [Fact]
    public async Task CreateValue_WithTwoCanonicalRepresentations_ReturnsValidationError()
    {
        IValueRepository valueRepository = Substitute.For<IValueRepository>();
        IPropertyRepository propertyRepository = Substitute.For<IPropertyRepository>();
        var property = new Property
        {
            Code = "connector",
            Name = "Connector",
            DataType = SpecificationDataType.Option
        };
        propertyRepository.GetPropertyByIdAsync(property.Id, Arg.Any<CancellationToken>())
            .Returns(property);
        var model = new CreateValueModel
        {
            PropertyId = property.Id,
            Text = "XT60",
            NumericValue = 60m,
            BooleanValue = true
        };

        Result<ValueModel> result = await new CreateValueCommandHandler(valueRepository, propertyRepository)
            .ExecuteCommandAsync(new CreateValueCommand(model), CancellationToken.None);

        Assert.True(result.HasError<ValidationError>());
    }

    [Fact]
    public async Task DeleteProperty_WhenUsedByComponentType_ReturnsConflict()
    {
        IPropertyRepository repository = Substitute.For<IPropertyRepository>();
        var property = new Property { Code = "motor-kv", Name = "Motor KV" };
        property.ComponentTypes.Add(new ComponentTypeProperty { PropertyId = property.Id });
        repository.GetPropertyByIdAsync(property.Id, Arg.Any<CancellationToken>()).Returns(property);

        Result result = await new DeletePropertyCommandHandler(repository)
            .ExecuteCommandAsync(new DeletePropertyCommand(property.Id), CancellationToken.None);

        Assert.True(result.HasError<ConflictError>());
        repository.DidNotReceive().RemoveProperty(property);
    }
}
