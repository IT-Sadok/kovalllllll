using DroneBuilder.Application.Mediator.Commands.ProductCommands;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using NSubstitute;

namespace DroneBuilder.Application.Tests.ProductCommandTests;

public class DeleteProductCommandHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly DeleteProductCommandHandler _handler;

    private static readonly Guid ProductId = Guid.NewGuid();
    private const string ProductName = "Test Product";

    public DeleteProductCommandHandlerTests()
    {
        // Arrange
        _productRepository = Substitute.For<IProductRepository>();
        _cartRepository = Substitute.For<ICartRepository>();
        _warehouseRepository = Substitute.For<IWarehouseRepository>();

        _handler = new DeleteProductCommandHandler(
            _productRepository,
            _cartRepository,
            _warehouseRepository);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenProductExists_ShouldDelistItInsteadOfDeletingTheRow()
    {
        // Arrange
        var command = new DeleteProductCommand(ProductId);

        var existingProduct = new Product
        {
            Id = ProductId,
            Name = ProductName
        };

        _productRepository.GetProductByIdAsync(
                Arg.Is<Guid>(id => id == ProductId),
                Arg.Any<CancellationToken>())
            .Returns(existingProduct);

        // Act
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.True(existingProduct.IsDeleted);

        await _productRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenProductExists_ShouldRemoveItFromEveryCart()
    {
        // Arrange
        var command = new DeleteProductCommand(ProductId);

        _productRepository.GetProductByIdAsync(
                Arg.Is<Guid>(id => id == ProductId),
                Arg.Any<CancellationToken>())
            .Returns(new Product { Id = ProductId, Name = ProductName });

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        await _cartRepository.Received(1).RemoveCartItemsByProductIdAsync(
            Arg.Is<Guid>(id => id == ProductId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenProductHasWarehouseItem_ShouldKeepItAndRestockCartReservations()
    {
        // Arrange
        var command = new DeleteProductCommand(ProductId);

        var warehouseItem = new WarehouseItem { ProductId = ProductId, Quantity = 5 };

        _productRepository.GetProductByIdAsync(
                Arg.Is<Guid>(id => id == ProductId),
                Arg.Any<CancellationToken>())
            .Returns(new Product { Id = ProductId, Name = ProductName });

        _cartRepository.RemoveCartItemsByProductIdAsync(
                Arg.Is<Guid>(id => id == ProductId),
                Arg.Any<CancellationToken>())
            .Returns(3);

        _warehouseRepository.GetWarehouseItemByProductIdAsync(
                Arg.Is<Guid>(id => id == ProductId),
                Arg.Any<CancellationToken>())
            .Returns(warehouseItem);

        // Act
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(8, warehouseItem.Quantity);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenProductHasNoWarehouseItem_ShouldStillSucceed()
    {
        // Arrange
        var command = new DeleteProductCommand(ProductId);

        _productRepository.GetProductByIdAsync(
                Arg.Is<Guid>(id => id == ProductId),
                Arg.Any<CancellationToken>())
            .Returns(new Product { Id = ProductId, Name = ProductName });

        _warehouseRepository.GetWarehouseItemByProductIdAsync(
                Arg.Is<Guid>(id => id == ProductId),
                Arg.Any<CancellationToken>())
            .Returns((WarehouseItem)null!);

        // Act
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        await _productRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenProductNotFound_ShouldReturnFailedResultWithNotFoundError()
    {
        // Arrange
        var command = new DeleteProductCommand(ProductId);

        _productRepository.GetProductByIdAsync(
                Arg.Is<Guid>(id => id == ProductId),
                Arg.Any<CancellationToken>())
            .Returns((Product)null!);

        // Act & Assert
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<NotFoundError>());

        Assert.Equal($"Product with id {ProductId} not found.", result.Errors[0].Message);

        await _cartRepository.DidNotReceive().RemoveCartItemsByProductIdAsync(
            Arg.Any<Guid>(), Arg.Any<CancellationToken>());

        await _productRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenProductIsAlreadyDelisted_ShouldReturnFailedResultWithNotFoundError()
    {
        // Arrange
        var command = new DeleteProductCommand(ProductId);

        _productRepository.GetProductByIdAsync(
                Arg.Is<Guid>(id => id == ProductId),
                Arg.Any<CancellationToken>())
            .Returns((Product)null!);

        // Act & Assert
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<NotFoundError>());
    }
}
