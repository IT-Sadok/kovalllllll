using DroneBuilder.Application.Features.Catalog.Images.GetImageById;
using DroneBuilder.Application.Features.Catalog.Images.GetImages;
using DroneBuilder.Application.Features.Catalog.Images.GetImagesByProductId;
using DroneBuilder.Application.Features.Inventory.GetWarehouse;
using DroneBuilder.Application.Features.Inventory.GetWarehouseItemById;
using DroneBuilder.Application.Features.Inventory.GetWarehouseItems;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Models.WarehouseModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using NSubstitute;

namespace DroneBuilder.Application.Tests.QueryHandlerTests;

public class MediaAndInventoryQueryHandlerTests
{
    [Fact]
    public async Task GetImageById_MissingImage_ReturnsNotFound()
    {
        IImageRepository images = Substitute.For<IImageRepository>();
        Guid id = Guid.NewGuid();
        images.GetImageByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Image?)null);

        Result<ImageModel> result = await new GetImageByIdQueryHandler(images)
            .ExecuteAsync(new GetImageByIdQuery(id), CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task GetImages_MapsRepositoryCollection()
    {
        IImageRepository images = Substitute.For<IImageRepository>();
        images.GetImagesAsync(Arg.Any<CancellationToken>())
            .Returns([new Image { Id = Guid.NewGuid(), Url = "https://example/image.webp" }]);

        Result<ICollection<ImageModel>> result = await new GetImagesQueryHandler(images)
            .ExecuteAsync(new GetImagesQuery(), CancellationToken.None);

        Assert.Equal("https://example/image.webp", Assert.Single(result.Value).Url);
    }

    [Fact]
    public async Task GetImagesByProductId_MissingProduct_ReturnsNotFound()
    {
        IProductRepository products = Substitute.For<IProductRepository>();
        Guid id = Guid.NewGuid();
        products.GetProductByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Product?)null);

        Result<ICollection<ImageModel>> result = await new GetImagesByProductIdQueryHandler(products)
            .ExecuteAsync(new GetImagesByProductIdQuery(id), CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task GetWarehouse_MissingWarehouse_ReturnsNotFound()
    {
        IWarehouseRepository warehouse = Substitute.For<IWarehouseRepository>();
        warehouse.GetWarehouseAsync(Arg.Any<CancellationToken>()).Returns((Warehouse?)null);

        Result<WarehouseModel> result = await new GetWarehouseQueryHandler(warehouse)
            .ExecuteAsync(new GetWarehouseQuery(), CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task GetWarehouseItemById_MapsAvailableQuantity()
    {
        IWarehouseRepository warehouse = Substitute.For<IWarehouseRepository>();
        var item = new WarehouseItem
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            Quantity = 12,
            ReservedQuantity = 5
        };
        warehouse.GetWarehouseItemByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);

        Result<WarehouseItemModel> result = await new GetWarehouseItemByIdQueryHandler(warehouse)
            .ExecuteAsync(new GetWarehouseItemByIdQuery(item.Id), CancellationToken.None);

        Assert.Equal(7, result.Value.Quantity);
    }

    [Fact]
    public async Task GetWarehouseItems_PreservesPaginationAndMapsAvailableQuantity()
    {
        IWarehouseRepository warehouse = Substitute.For<IWarehouseRepository>();
        var pagination = new PaginationParams(1, 10);
        warehouse.GetWarehouseItemsAsync(pagination, Arg.Any<CancellationToken>())
            .Returns(new PagedResult<WarehouseItem>
            {
                Items = [new WarehouseItem { Quantity = 9, ReservedQuantity = 2 }],
                TotalCount = 1,
                Page = 1,
                PageSize = 10
            });

        Result<PagedResult<WarehouseItemModel>> result = await new GetWarehouseItemsQueryHandler(warehouse)
            .ExecuteAsync(new GetWarehouseItemsQuery(pagination), CancellationToken.None);

        Assert.Equal(1, result.Value.TotalCount);
        Assert.Equal(7, Assert.Single(result.Value.Items).Quantity);
    }
}
