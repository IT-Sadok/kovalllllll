using DroneBuilder.Application.Features.Catalog.Metadata.GetComponentTypeProperties;
using DroneBuilder.Application.Features.Catalog.Metadata.GetComponentTypes;
using DroneBuilder.Application.Features.Catalog.Metadata.GetUnits;
using DroneBuilder.Application.Features.Catalog.Metadata.Models;
using DroneBuilder.Application.Features.Catalog.Products.AssignComponentType;
using DroneBuilder.Application.Features.Catalog.ProductSpecifications.CreateProductSpecification;
using DroneBuilder.Application.Features.Catalog.ProductSpecifications.DeleteProductSpecification;
using DroneBuilder.Application.Features.Catalog.ProductSpecifications.GetProductSpecifications;
using DroneBuilder.Application.Features.Catalog.ProductSpecifications.Models;
using DroneBuilder.Application.Features.Catalog.ProductSpecifications.UpdateProductSpecification;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using NSubstitute;
using DomainValue = DroneBuilder.Domain.Entities.Value;

namespace DroneBuilder.Application.Tests.CatalogSpecificationTests;

public class CatalogSpecificationHandlerTests
{
    private readonly ICatalogSpecificationRepository _repository =
        Substitute.For<ICatalogSpecificationRepository>();

    [Fact]
    public async Task GetComponentTypes_ReturnsActiveMetadata()
    {
        _repository.GetComponentTypesAsync(Arg.Any<CancellationToken>())
            .Returns([new ComponentType { Code = "motor", Name = "Motor" }]);

        Result<ICollection<ComponentTypeModel>> result =
            await new GetComponentTypesQueryHandler(_repository)
                .ExecuteAsync(new GetComponentTypesQuery(), CancellationToken.None);

        Assert.Equal("motor", Assert.Single(result.Value).Code);
    }

    [Fact]
    public async Task GetComponentTypeProperties_ReturnsRulesAndOptions()
    {
        var option = new DomainValue { Code = "six-s", Text = "6S" };
        var property = new Property
        {
            Code = "battery-cells",
            Name = "Battery cells",
            DataType = SpecificationDataType.Option,
            Values = [option]
        };
        var componentType = new ComponentType { Code = "battery", Name = "Battery" };
        componentType.Properties.Add(new ComponentTypeProperty
        {
            ComponentTypeId = componentType.Id,
            PropertyId = property.Id,
            Property = property,
            IsRequired = true,
            SortOrder = 1
        });
        _repository.GetComponentTypeAsync(componentType.Id, Arg.Any<CancellationToken>())
            .Returns(componentType);

        Result<ComponentTypeDetailsModel> result =
            await new GetComponentTypePropertiesQueryHandler(_repository)
                .ExecuteAsync(
                    new GetComponentTypePropertiesQuery(componentType.Id),
                    CancellationToken.None);

        ComponentTypePropertyModel rule = Assert.Single(result.Value.Properties);
        Assert.True(rule.IsRequired);
        Assert.Equal("Option", rule.DataType);
        Assert.Null(rule.Unit);
        Assert.Equal("6S", Assert.Single(rule.Options).Text);
    }

    [Fact]
    public async Task GetUnits_ReturnsAliases()
    {
        var unit = new UnitDefinition { Code = "millimeter", Name = "Millimeter", Symbol = "mm" };
        unit.Aliases.Add(new UnitAlias { Alias = "millimeters" });
        _repository.GetUnitsAsync(Arg.Any<CancellationToken>()).Returns([unit]);

        Result<ICollection<UnitDefinitionModel>> result = await new GetUnitsQueryHandler(_repository)
            .ExecuteAsync(new GetUnitsQuery(), CancellationToken.None);

        Assert.Equal("millimeters", Assert.Single(result.Value).Aliases.Single());
    }

    [Fact]
    public async Task AssignComponentType_SetsProductKindAndSaves()
    {
        Product product = CreateProductWithoutComponentType();
        ComponentType componentType = CreateComponentTypeWithNumberProperty(out _);
        _repository.GetProductAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _repository.GetComponentTypeAsync(componentType.Id, Arg.Any<CancellationToken>())
            .Returns(componentType);

        Result<ProductComponentTypeModel> result =
            await new AssignProductComponentTypeCommandHandler(_repository)
                .ExecuteCommandAsync(
                    new AssignProductComponentTypeCommand(
                        product.Id,
                        new AssignProductComponentTypeModel { ComponentTypeId = componentType.Id }),
                    CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ProductKind.Component, product.Kind);
        Assert.Same(componentType, product.ComponentType);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AssignComponentType_WithIncompatibleExistingSpecification_ReturnsConflict()
    {
        Product product = CreateProductWithoutComponentType();
        product.ProductPropertyValues.Add(new ProductPropertyValue { PropertyId = Guid.NewGuid() });
        ComponentType componentType = CreateComponentTypeWithNumberProperty(out _);
        _repository.GetProductAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _repository.GetComponentTypeAsync(componentType.Id, Arg.Any<CancellationToken>())
            .Returns(componentType);

        Result<ProductComponentTypeModel> result =
            await new AssignProductComponentTypeCommandHandler(_repository)
                .ExecuteCommandAsync(
                    new AssignProductComponentTypeCommand(
                        product.Id,
                        new AssignProductComponentTypeModel { ComponentTypeId = componentType.Id }),
                    CancellationToken.None);

        Assert.True(result.HasError<ConflictError>());
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetProductSpecifications_MapsTypedValue()
    {
        Product product = CreateProductWithNumberProperty(out Property property);
        product.ProductPropertyValues.Add(new ProductPropertyValue
        {
            ProductId = product.Id,
            PropertyId = property.Id,
            Property = property,
            NumericValue = 1750m
        });
        _repository.GetProductAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        Result<ICollection<ProductSpecificationModel>> result =
            await new GetProductSpecificationsQueryHandler(_repository)
                .ExecuteAsync(new GetProductSpecificationsQuery(product.Id), CancellationToken.None);

        ProductSpecificationModel specification = Assert.Single(result.Value);
        Assert.Equal("motor-kv", specification.PropertyCode);
        Assert.Equal(1750m, specification.NumericValue);
    }

    [Fact]
    public async Task CreateProductSpecification_WithNumber_AddsAndSaves()
    {
        Product product = CreateProductWithNumberProperty(out Property property);
        _repository.GetProductAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _repository.GetPropertyAsync(property.Id, Arg.Any<CancellationToken>()).Returns(property);
        var model = new CreateProductSpecificationModel
        {
            PropertyId = property.Id,
            NumericValue = 1750m
        };

        Result<ProductSpecificationModel> result =
            await new CreateProductSpecificationCommandHandler(_repository)
                .ExecuteCommandAsync(
                    new CreateProductSpecificationCommand(product.Id, model),
                    CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1750m, Assert.Single(product.ProductPropertyValues).NumericValue);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateProductSpecification_WithoutComponentType_ReturnsConflict()
    {
        Product product = CreateProductWithoutComponentType();
        _repository.GetProductAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        Result<ProductSpecificationModel> result =
            await new CreateProductSpecificationCommandHandler(_repository)
                .ExecuteCommandAsync(
                    new CreateProductSpecificationCommand(
                        product.Id,
                        new CreateProductSpecificationModel
                        {
                            PropertyId = Guid.NewGuid(),
                            NumericValue = 1m
                        }),
                    CancellationToken.None);

        Assert.True(result.HasError<ConflictError>());
    }

    [Fact]
    public async Task CreateProductSpecification_WithOptionFromAnotherProperty_ReturnsValidationError()
    {
        Product product = CreateProductWithOptionProperty(out Property property);
        _repository.GetProductAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _repository.GetPropertyAsync(property.Id, Arg.Any<CancellationToken>()).Returns(property);

        Result<ProductSpecificationModel> result =
            await new CreateProductSpecificationCommandHandler(_repository)
                .ExecuteCommandAsync(
                    new CreateProductSpecificationCommand(
                        product.Id,
                        new CreateProductSpecificationModel
                        {
                            PropertyId = property.Id,
                            ValueId = Guid.NewGuid()
                        }),
                    CancellationToken.None);

        Assert.True(result.HasError<ValidationError>());
        Assert.Empty(product.ProductPropertyValues);
    }

    [Fact]
    public async Task UpdateProductSpecification_WithWrongRepresentation_PreservesExistingValue()
    {
        Product product = CreateProductWithNumberProperty(out Property property);
        var specification = new ProductPropertyValue
        {
            ProductId = product.Id,
            PropertyId = property.Id,
            Property = property,
            NumericValue = 1750m
        };
        product.ProductPropertyValues.Add(specification);
        _repository.GetProductAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        Result<ProductSpecificationModel> result =
            await new UpdateProductSpecificationCommandHandler(_repository)
                .ExecuteCommandAsync(
                    new UpdateProductSpecificationCommand(
                        product.Id,
                        specification.Id,
                        new UpdateProductSpecificationModel { TextValue = "invalid" }),
                    CancellationToken.None);

        Assert.True(result.HasError<ValidationError>());
        Assert.Equal(1750m, specification.NumericValue);
        Assert.Null(specification.TextValue);
    }

    [Fact]
    public async Task DeleteProductSpecification_WhenRequiredAndLast_ReturnsConflict()
    {
        Product product = CreateProductWithNumberProperty(out Property property, required: true);
        var specification = new ProductPropertyValue
        {
            ProductId = product.Id,
            PropertyId = property.Id,
            Property = property,
            NumericValue = 1750m
        };
        product.ProductPropertyValues.Add(specification);
        _repository.GetProductAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        Result result = await new DeleteProductSpecificationCommandHandler(_repository)
            .ExecuteCommandAsync(
                new DeleteProductSpecificationCommand(product.Id, specification.Id),
                CancellationToken.None);

        Assert.True(result.HasError<ConflictError>());
        _repository.DidNotReceive().RemoveSpecification(Arg.Any<ProductPropertyValue>());
    }

    [Fact]
    public async Task DeleteProductSpecification_WhenOptional_RemovesAndSaves()
    {
        Product product = CreateProductWithNumberProperty(out Property property);
        var specification = new ProductPropertyValue
        {
            ProductId = product.Id,
            PropertyId = property.Id,
            Property = property,
            NumericValue = 1750m
        };
        product.ProductPropertyValues.Add(specification);
        _repository.GetProductAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        Result result = await new DeleteProductSpecificationCommandHandler(_repository)
            .ExecuteCommandAsync(
                new DeleteProductSpecificationCommand(product.Id, specification.Id),
                CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repository.Received(1).RemoveSpecification(specification);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static Product CreateProductWithoutComponentType()
    {
        return new Product { Name = "Test product" };
    }

    private static ComponentType CreateComponentTypeWithNumberProperty(out Property property, bool required = false)
    {
        property = new Property
        {
            Code = "motor-kv",
            Name = "Motor KV",
            DataType = SpecificationDataType.Number,
            AllowsMultipleValues = false
        };
        var componentType = new ComponentType { Code = "motor", Name = "Motor" };
        componentType.Properties.Add(new ComponentTypeProperty
        {
            ComponentTypeId = componentType.Id,
            PropertyId = property.Id,
            Property = property,
            IsRequired = required,
            IsVariantSpecific = false
        });
        return componentType;
    }

    private static Product CreateProductWithNumberProperty(out Property property, bool required = false)
    {
        ComponentType componentType = CreateComponentTypeWithNumberProperty(out property, required);
        return new Product
        {
            Name = "Motor",
            Kind = ProductKind.Component,
            ComponentTypeId = componentType.Id,
            ComponentType = componentType
        };
    }

    private static Product CreateProductWithOptionProperty(out Property property)
    {
        property = new Property
        {
            Code = "connector",
            Name = "Connector",
            DataType = SpecificationDataType.Option
        };
        property.Values.Add(new DomainValue { Code = "xt60", Text = "XT60" });
        var componentType = new ComponentType { Code = "battery", Name = "Battery" };
        componentType.Properties.Add(new ComponentTypeProperty
        {
            ComponentTypeId = componentType.Id,
            PropertyId = property.Id,
            Property = property
        });
        return new Product
        {
            Name = "Battery",
            Kind = ProductKind.Component,
            ComponentTypeId = componentType.Id,
            ComponentType = componentType
        };
    }
}
