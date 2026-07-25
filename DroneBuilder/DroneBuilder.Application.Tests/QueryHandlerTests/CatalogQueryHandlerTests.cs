using DroneBuilder.Application.Features.Catalog.Products.GetCategories;
using DroneBuilder.Application.Features.Catalog.Products.GetProductById;
using DroneBuilder.Application.Features.Catalog.Products.GetProducts;
using DroneBuilder.Application.Features.Catalog.Products.GetPropertiesByProductId;
using DroneBuilder.Application.Features.Catalog.Properties.GetProperties;
using DroneBuilder.Application.Features.Catalog.Properties.GetPropertyById;
using DroneBuilder.Application.Features.Catalog.Properties.GetValuesByPropertyId;
using DroneBuilder.Application.Features.Catalog.Values.GetValueById;
using DroneBuilder.Application.Features.Catalog.Values.GetValues;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using NSubstitute;
using DomainValue = DroneBuilder.Domain.Entities.Value;

namespace DroneBuilder.Application.Tests.QueryHandlerTests;

public class CatalogQueryHandlerTests
{
    [Fact]
    public async Task GetProducts_MapsPaginationAndAvailableStock()
    {
        IProductRepository products = Substitute.For<IProductRepository>();
        IWarehouseRepository warehouse = Substitute.For<IWarehouseRepository>();
        Guid productId = Guid.NewGuid();
        var pagination = new PaginationParams(2, 10);
        var filter = new ProductFilterModel();
        products.GetFilteredPagedProductsAsync(pagination, filter, Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Product>
            {
                Items = [new Product { Id = productId, Name = "Motor", Price = 50m }],
                TotalCount = 21,
                Page = 2,
                PageSize = 10
            });
        warehouse.GetAllWarehouseItemsByProductIdsAsync(
                Arg.Any<ICollection<Guid>>(),
                Arg.Any<CancellationToken>())
            .Returns([
                new WarehouseItem { ProductId = productId, Quantity = 8, ReservedQuantity = 3 },
                new WarehouseItem { ProductId = productId, Quantity = 4, ReservedQuantity = 1 }
            ]);

        Result<PagedResult<ProductModel>> result = await new GetProductsQueryHandler(products, warehouse)
            .ExecuteAsync(new GetProductsQuery(pagination, filter), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(21, result.Value.TotalCount);
        Assert.Equal(8, Assert.Single(result.Value.Items).StockQuantity);
    }

    [Fact]
    public async Task GetProductById_MissingProduct_ReturnsNotFound()
    {
        IProductRepository products = Substitute.For<IProductRepository>();
        IWarehouseRepository warehouse = Substitute.For<IWarehouseRepository>();
        Guid id = Guid.NewGuid();
        products.GetProductByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Product?)null);

        Result<ProductModel> result = await new GetProductByIdQueryHandler(products, warehouse)
            .ExecuteAsync(new GetProductByIdQuery(id), CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
        await warehouse.DidNotReceive().GetWarehouseItemByProductIdAsync(
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPropertiesByProductId_UsesAvailableStock()
    {
        IProductRepository products = Substitute.For<IProductRepository>();
        IWarehouseRepository warehouse = Substitute.For<IWarehouseRepository>();
        var product = new Product { Id = Guid.NewGuid(), Name = "Frame" };
        products.GetPropertiesByProductIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        warehouse.GetWarehouseItemByProductIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(new WarehouseItem { ProductId = product.Id, Quantity = 10, ReservedQuantity = 6 });

        Result<ProductPropertiesResponseModel> result =
            await new GetPropertiesByProductIdQueryHandler(products, warehouse)
                .ExecuteAsync(new GetPropertiesByProductIdQuery(product.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.Value.StockQuantity);
    }

    [Fact]
    public async Task GetCategories_ReturnsRepositoryValues()
    {
        IProductRepository products = Substitute.For<IProductRepository>();
        products.GetCategoriesAsync(Arg.Any<CancellationToken>()).Returns(["Motors", "Frames"]);

        Result<IEnumerable<string>> result = await new GetCategoriesQueryHandler(products)
            .ExecuteAsync(new GetCategoriesQuery(), CancellationToken.None);

        Assert.Equal(["Motors", "Frames"], result.Value);
    }

    [Fact]
    public async Task GetProperties_MapsRepositoryEntities()
    {
        IPropertyRepository properties = Substitute.For<IPropertyRepository>();
        properties.GetPropertiesAsync(Arg.Any<CancellationToken>())
            .Returns([new Property { Id = Guid.NewGuid(), Code = "kv", Name = "KV" }]);

        Result<ICollection<PropertyModel>> result = await new GetPropertiesQueryHandler(properties)
            .ExecuteAsync(new GetPropertiesQuery(), CancellationToken.None);

        Assert.Equal("KV", Assert.Single(result.Value).Name);
    }

    [Fact]
    public async Task GetPropertyById_MissingProperty_ReturnsNotFound()
    {
        IPropertyRepository properties = Substitute.For<IPropertyRepository>();
        Guid id = Guid.NewGuid();
        properties.GetPropertyByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Property?)null);

        Result<PropertyModel> result = await new GetPropertyByIdQueryHandler(properties)
            .ExecuteAsync(new GetPropertyByIdQuery(id), CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task GetValuesByPropertyId_MissingProperty_ReturnsNotFound()
    {
        IPropertyRepository properties = Substitute.For<IPropertyRepository>();
        Guid id = Guid.NewGuid();
        properties.GetValuesByPropertyIdAsync(id, Arg.Any<CancellationToken>()).Returns((Property?)null);

        Result<PropertyModel> result = await new GetValuesByPropertyIdQueryHandler(properties)
            .ExecuteAsync(new GetValuesByPropertyIdQuery(id), CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task GetValueById_MapsValue()
    {
        IValueRepository values = Substitute.For<IValueRepository>();
        var value = new DomainValue { Id = Guid.NewGuid(), Code = "six-s", Text = "6S" };
        values.GetValueByIdAsync(value.Id, Arg.Any<CancellationToken>()).Returns(value);

        Result<ValueModel> result = await new GetValueByIdQueryHandler(values)
            .ExecuteAsync(new GetValueByIdQuery(value.Id), CancellationToken.None);

        Assert.Equal("6S", result.Value.Text);
    }

    [Fact]
    public async Task GetValues_MapsEmptyCollectionAsSuccessfulResponse()
    {
        IValueRepository values = Substitute.For<IValueRepository>();
        values.GetValuesAsync(Arg.Any<CancellationToken>()).Returns([]);

        Result<ICollection<ValueModel>> result = await new GetValuesQueryHandler(values)
            .ExecuteAsync(new GetValuesQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }
}
