using DroneBuilder.Application.Features.Catalog.Products.DeleteProduct;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using NSubstitute;

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
            .Returns((Product?)null);

        // Act & Assert
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<NotFoundError>());

        Assert.Equal($"Product with id {ProductId} not found.", result.Errors[0].Message);

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

        Product? removedProduct = null;
        _productRepository.When(x => x.RemoveProduct(Arg.Any<Product>()))
            .Do(callInfo => removedProduct = callInfo.Arg<Product>());

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(removedProduct);
        Assert.Same(existingProduct, removedProduct);
    }
}
