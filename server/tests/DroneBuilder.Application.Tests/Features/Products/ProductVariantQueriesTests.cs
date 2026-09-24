using DroneBuilder.Application.Common.Pagination;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Features.Products;
using DroneBuilder.Application.Features.Products.GetProductById;
using DroneBuilder.Application.Features.Products.GetProducts;
using DroneBuilder.Domain.Entities;
using FluentResults;
using NSubstitute;

namespace DroneBuilder.Application.Tests.Features.Products;

public class ProductVariantQueriesTests
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly IWarehouseRepository _warehouseRepository = Substitute.For<IWarehouseRepository>();

    private static readonly Guid GroupId = Guid.NewGuid();

    private static Product Variant(string name, decimal price) =>
        new() { Name = $"RDQ Badass 2 - {name}", VariantName = name, Price = price, GroupId = GroupId };

    [Fact]
    public async Task GetProductById_WhenProductIsInGroup_ShouldListSiblingVariantsInNaturalOrder()
    {
        // Arrange
        Product v2400 = Variant("2400Kv", 14.29m);
        Product v1400 = Variant("1400Kv", 14.29m);
        Product v1900 = Variant("1900Kv", 15.99m);

        _productRepository.GetProductByIdAsync(v1900.Id, Arg.Any<CancellationToken>()).Returns(v1900);
        _productRepository.GetGroupVariantsAsync(GroupId, Arg.Any<CancellationToken>()).Returns([v2400, v1400, v1900]);
        _productRepository.GetGroupSummariesAsync(Arg.Any<ICollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([new ProductGroupSummary(GroupId, "RDQ Badass 2", 3, 14.29m, 15.99m, 7)]);
        _warehouseRepository.GetAllWarehouseItemsByProductIdsAsync(Arg.Any<ICollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([new WarehouseItem { ProductId = v1400.Id, Quantity = 7 }]);

        var handler = new GetProductByIdQueryHandler(_productRepository, _warehouseRepository);

        // Act
        Result<ProductModel> result = await handler.ExecuteAsync(new GetProductByIdQuery(v1900.Id), CancellationToken.None);

        // Assert
        ProductGroupModel group = result.Value.Group!;
        Assert.Equal("RDQ Badass 2", group.Name);
        Assert.Equal(3, group.VariantCount);
        Assert.Equal((14.29m, 15.99m), (group.MinPrice, group.MaxPrice));
        Assert.Equal(["1400Kv", "1900Kv", "2400Kv"], group.Variants!.Select(v => v.VariantName));
        Assert.Equal(7, group.Variants![0].StockQuantity);
        Assert.Equal(0, group.Variants[2].StockQuantity);
    }

    [Fact]
    public async Task GetProductById_WhenProductHasNoGroup_ShouldNotQueryVariants()
    {
        // Arrange
        var product = new Product { Name = "Frame" };
        _productRepository.GetProductByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        var handler = new GetProductByIdQueryHandler(_productRepository, _warehouseRepository);

        // Act
        Result<ProductModel> result = await handler.ExecuteAsync(new GetProductByIdQuery(product.Id), CancellationToken.None);

        // Assert
        Assert.Null(result.Value.Group);
        await _productRepository.DidNotReceive().GetGroupVariantsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetProducts_WhenItemsBelongToGroups_ShouldAttachGroupSummaries()
    {
        // Arrange
        Product grouped = Variant("1400Kv", 14.29m);
        var single = new Product { Name = "Frame" };
        var filter = new ProductFilterModel { CollapseVariants = true };

        _productRepository.GetFilteredPagedProductsAsync(Arg.Any<PaginationParams>(), filter, Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Product> { Items = [grouped, single], TotalCount = 2, Page = 1, PageSize = 20 });
        _productRepository.GetGroupSummariesAsync(
                Arg.Is<ICollection<Guid>>(ids => ids.Single() == GroupId), Arg.Any<CancellationToken>())
            .Returns([new ProductGroupSummary(GroupId, "RDQ Badass 2", 3, 14.29m, 15.99m, 12)]);
        _warehouseRepository.GetAllWarehouseItemsByProductIdsAsync(Arg.Any<ICollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var handler = new GetProductsQueryHandler(_productRepository, _warehouseRepository);

        // Act
        Result<PagedResult<ProductModel>> result =
            await handler.ExecuteAsync(new GetProductsQuery(new PaginationParams(1, 20), filter), CancellationToken.None);

        // Assert
        ProductModel first = result.Value.Items.First();
        Assert.Equal(3, first.Group!.VariantCount);
        Assert.Equal(12, first.Group.TotalStock);
        Assert.Null(first.Group.Variants);
        Assert.Null(result.Value.Items.Last().Group);
    }
}
