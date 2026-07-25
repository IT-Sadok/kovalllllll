using DroneBuilder.Application.Features.Catalog.ProductVariants.CreateProductVariant;
using DroneBuilder.Application.Features.Catalog.ProductVariants.CreateVariantSpecification;
using DroneBuilder.Application.Features.Catalog.ProductVariants.DeleteProductVariant;
using DroneBuilder.Application.Features.Catalog.ProductVariants.Models;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using NSubstitute;

namespace DroneBuilder.Application.Tests.CatalogSpecificationTests;

public class ProductVariantAndPublicationTests
{
    private readonly ICatalogSpecificationRepository _catalogRepository =
        Substitute.For<ICatalogSpecificationRepository>();
    private readonly IWarehouseRepository _warehouseRepository =
        Substitute.For<IWarehouseRepository>();

    [Fact]
    public void AssignComponentType_MovesPublishedProductToDraft()
    {
        Product product = CreateProduct();
        var componentType = new ComponentType { Code = "motor", Name = "Motor" };

        product.AssignComponentType(componentType);

        Assert.Equal(ProductPublicationStatus.Draft, product.PublicationStatus);
        Assert.Equal(ProductKind.Component, product.Kind);
    }

    [Fact]
    public void Publish_WithMissingRequiredSpecification_ThrowsAndRemainsDraft()
    {
        Product product = CreateProduct();
        Property property = CreateNumberProperty();
        ComponentType componentType = CreateComponentType(property, required: true, variantSpecific: false);
        product.AssignComponentType(componentType);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(product.Publish);

        Assert.Contains("Required property", exception.Message);
        Assert.Equal(ProductPublicationStatus.Draft, product.PublicationStatus);
    }

    [Fact]
    public void Publish_WithCompleteRequiredSpecification_Succeeds()
    {
        Product product = CreateProduct();
        Property property = CreateNumberProperty();
        ComponentType componentType = CreateComponentType(property, required: true, variantSpecific: false);
        product.AssignComponentType(componentType);
        var specification = new ProductPropertyValue
        {
            Property = property,
            PropertyId = property.Id,
            NumericValue = 1750m
        };
        product.AddSpecification(specification);

        product.Publish();

        Assert.Equal(ProductPublicationStatus.Published, product.PublicationStatus);
    }

    [Fact]
    public async Task CreateVariant_AddsWarehouseItemAndMovesProductToDraft()
    {
        Product product = CreateProduct();
        var warehouse = new Warehouse { Code = "main", Name = "Main" };
        _catalogRepository.GetProductAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _catalogRepository.IsSkuInUseAsync("MOTOR-2", null, Arg.Any<CancellationToken>()).Returns(false);
        _warehouseRepository.GetWarehouseAsync(Arg.Any<CancellationToken>()).Returns(warehouse);
        var model = new CreateProductVariantModel
        {
            Sku = "MOTOR-2",
            Name = "Second",
            Price = 40m,
            CurrencyCode = "usd"
        };

        Result<ProductVariantModel> result = await new CreateProductVariantCommandHandler(
                _catalogRepository,
                _warehouseRepository)
            .ExecuteCommandAsync(
                new CreateProductVariantCommand(product.Id, model),
                CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ProductPublicationStatus.Draft, product.PublicationStatus);
        ProductVariant created = product.Variants.Single(variant => variant.Sku == "MOTOR-2");
        Assert.Equal("USD", created.CurrencyCode);
        await _warehouseRepository.Received(1).AddWarehouseItemAsync(
            Arg.Is<WarehouseItem>(item => item.ProductVariantId == created.Id && item.WarehouseId == warehouse.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateVariantSpecification_WithVariantRule_AddsTypedValue()
    {
        Product product = CreateProduct();
        Property property = CreateNumberProperty();
        product.AssignComponentType(CreateComponentType(property, required: false, variantSpecific: true));
        ProductVariant variant = product.Variants.Single();
        _catalogRepository.GetProductAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _catalogRepository.GetPropertyAsync(property.Id, Arg.Any<CancellationToken>()).Returns(property);
        var model = new CreateProductVariantSpecificationModel
        {
            PropertyId = property.Id,
            NumericValue = 1950m
        };

        Result<ProductVariantSpecificationModel> result =
            await new CreateVariantSpecificationCommandHandler(_catalogRepository)
                .ExecuteCommandAsync(
                    new CreateVariantSpecificationCommand(product.Id, variant.Id, model),
                    CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1950m, Assert.Single(variant.Specifications).NumericValue);
        await _catalogRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteDefaultVariant_ReturnsConflict()
    {
        Product product = CreateProduct();
        ProductVariant defaultVariant = product.Variants.Single();
        _catalogRepository.GetProductAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        Result result = await new DeleteProductVariantCommandHandler(_catalogRepository)
            .ExecuteCommandAsync(
                new DeleteProductVariantCommand(product.Id, defaultVariant.Id),
                CancellationToken.None);

        Assert.True(result.HasError<ConflictError>());
        Assert.True(defaultVariant.IsActive);
    }

    private static Product CreateProduct()
    {
        var product = new Product { Name = "Motor" };
        product.EnsureDefaultVariant("MOTOR-1");
        return product;
    }

    private static Property CreateNumberProperty()
    {
        return new Property
        {
            Code = "motor-kv",
            Name = "Motor KV",
            DataType = SpecificationDataType.Number,
            AllowsMultipleValues = false
        };
    }

    private static ComponentType CreateComponentType(
        Property property,
        bool required,
        bool variantSpecific)
    {
        var componentType = new ComponentType { Code = "motor", Name = "Motor" };
        componentType.Properties.Add(new ComponentTypeProperty
        {
            ComponentTypeId = componentType.Id,
            PropertyId = property.Id,
            Property = property,
            IsRequired = required,
            IsVariantSpecific = variantSpecific
        });
        return componentType;
    }
}
