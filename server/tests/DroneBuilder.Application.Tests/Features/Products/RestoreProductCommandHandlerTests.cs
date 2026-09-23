using DroneBuilder.Application.Features.Products.RestoreProduct;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using NSubstitute;

namespace DroneBuilder.Application.Tests.Features.Products;

public class RestoreProductCommandHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly RestoreProductCommandHandler _handler;

    private static readonly Guid ProductId = Guid.NewGuid();
    private static readonly Guid WarehouseId = Guid.NewGuid();
    private const string ProductName = "Test Product";

    public RestoreProductCommandHandlerTests()
    {
        // Arrange
        _productRepository = Substitute.For<IProductRepository>();
        _warehouseRepository = Substitute.For<IWarehouseRepository>();

        _warehouseRepository.GetWarehouseAsync(Arg.Any<CancellationToken>())
            .Returns(new Warehouse { Id = WarehouseId });

        _handler = new RestoreProductCommandHandler(_productRepository, _warehouseRepository);
    }

    private void GivenDelistedProduct(Product product)
        => _productRepository.GetDelistedProductByIdAsync(
                Arg.Is<Guid>(id => id == ProductId),
                Arg.Any<CancellationToken>())
            .Returns(product);

    [Fact]
    public async Task ExecuteCommandAsync_WhenProductIsDelisted_ShouldPutItBackInTheCatalogue()
    {
        // Arrange
        var product = new Product { Id = ProductId, Name = ProductName, IsDeleted = true };
        GivenDelistedProduct(product);

        // Act
        Result result = await _handler.ExecuteCommandAsync(new RestoreProductCommand(ProductId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(product.IsDeleted);

        await _productRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenRestoring_ShouldRecreateTheWarehouseRecordAtZero()
    {
        // Arrange
        GivenDelistedProduct(new Product { Id = ProductId, Name = ProductName, IsDeleted = true });

        _warehouseRepository.GetWarehouseItemByProductIdAsync(
                Arg.Is<Guid>(id => id == ProductId),
                Arg.Any<CancellationToken>())
            .Returns((WarehouseItem)null!);

        // Act
        await _handler.ExecuteCommandAsync(new RestoreProductCommand(ProductId), CancellationToken.None);

        // Assert
        await _warehouseRepository.Received(1).AddWarehouseItemAsync(
            Arg.Is<WarehouseItem>(wi =>
                wi.ProductId == ProductId &&
                wi.WarehouseId == WarehouseId &&
                wi.Quantity == 0),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenWarehouseRecordSurvived_ShouldNotCreateASecondOne()
    {
        // Arrange
        GivenDelistedProduct(new Product { Id = ProductId, Name = ProductName, IsDeleted = true });

        _warehouseRepository.GetWarehouseItemByProductIdAsync(
                Arg.Is<Guid>(id => id == ProductId),
                Arg.Any<CancellationToken>())
            .Returns(new WarehouseItem { ProductId = ProductId, Quantity = 4 });

        // Act
        Result result = await _handler.ExecuteCommandAsync(new RestoreProductCommand(ProductId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        await _warehouseRepository.DidNotReceive().AddWarehouseItemAsync(
            Arg.Any<WarehouseItem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenProductIsNotDelisted_ShouldReturnFailedResultWithNotFoundError()
    {
        // Arrange
        GivenDelistedProduct(null!);

        // Act & Assert
        Result result = await _handler.ExecuteCommandAsync(new RestoreProductCommand(ProductId), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<NotFoundError>());

        await _productRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenWarehouseMissing_ShouldReturnFailedResultWithNotFoundError()
    {
        // Arrange
        var product = new Product { Id = ProductId, Name = ProductName, IsDeleted = true };
        GivenDelistedProduct(product);

        _warehouseRepository.GetWarehouseAsync(Arg.Any<CancellationToken>()).Returns((Warehouse)null!);

        // Act & Assert
        Result result = await _handler.ExecuteCommandAsync(new RestoreProductCommand(ProductId), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<NotFoundError>());

        Assert.True(product.IsDeleted);

        await _productRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
