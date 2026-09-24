using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Features.Products.RemoveProductSpec;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Entities.Components;
using FluentResults;
using NSubstitute;

namespace DroneBuilder.Application.Tests.Features.Products;

public class RemoveProductSpecCommandHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly RemoveProductSpecCommandHandler _handler;

    private static readonly Guid ProductId = Guid.NewGuid();

    public RemoveProductSpecCommandHandlerTests()
    {
        // Arrange
        _productRepository = Substitute.For<IProductRepository>();

        _handler = new RemoveProductSpecCommandHandler(_productRepository);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenProductHasSpec_ShouldRemoveSpecAndSave()
    {
        // Arrange
        var product = new Product { Id = ProductId, Spec = new AntennaSpec { ProductId = ProductId } };

        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(product);

        // Act
        Result result = await _handler.ExecuteCommandAsync(new RemoveProductSpecCommand(ProductId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(product.Spec);
        await _productRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenProductHasNoSpec_ShouldReturnNotFound()
    {
        // Arrange
        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(new Product { Id = ProductId });

        // Act
        Result result = await _handler.ExecuteCommandAsync(new RemoveProductSpecCommand(ProductId), CancellationToken.None);

        // Assert
        Assert.True(result.HasError<NotFoundError>());
        await _productRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenProductNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        // Act
        Result result = await _handler.ExecuteCommandAsync(new RemoveProductSpecCommand(ProductId), CancellationToken.None);

        // Assert
        Assert.True(result.HasError<NotFoundError>());
    }
}
