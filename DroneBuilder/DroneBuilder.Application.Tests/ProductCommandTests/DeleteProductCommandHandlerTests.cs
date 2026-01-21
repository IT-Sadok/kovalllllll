using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Commands.ProductCommands;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using NSubstitute;
using Xunit;

namespace DroneBuilder.Application.Tests.ProductCommandTests;

public class DeleteProductCommandHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly DeleteProductCommandHandler _handler;

    private static readonly Guid ProductId = Guid.NewGuid();
    private const string ProductName = "Test Product";

    public DeleteProductCommandHandlerTests()
    {
        // Arrange
        _productRepository = Substitute.For<IProductRepository>();

        _handler = new DeleteProductCommandHandler(_productRepository);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenProductExists_ShouldDeleteProduct()
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
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        _productRepository.Received(1).RemoveProduct(
            Arg.Is<Product>(p => p.Id == ProductId));

        await _productRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new DeleteProductCommand(ProductId);

        _productRepository.GetProductByIdAsync(
                Arg.Is<Guid>(id => id == ProductId),
                Arg.Any<CancellationToken>())
            .Returns((Product)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal($"Product with id {ProductId} not found.", exception.Message);

        _productRepository.DidNotReceive().RemoveProduct(Arg.Any<Product>());

        await _productRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenSuccessful_ShouldRemoveExactProduct()
    {
        // Arrange
        var command = new DeleteProductCommand(ProductId);

        var existingProduct = new Product
        {
            Id = ProductId,
            Name = ProductName,
            Price = 100m
        };

        _productRepository.GetProductByIdAsync(
                Arg.Is<Guid>(id => id == ProductId),
                Arg.Any<CancellationToken>())
            .Returns(existingProduct);

        Product removedProduct = null;
        _productRepository.When(x => x.RemoveProduct(Arg.Any<Product>()))
            .Do(callInfo => removedProduct = callInfo.Arg<Product>());

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(removedProduct);
        Assert.Same(existingProduct, removedProduct);
    }
}