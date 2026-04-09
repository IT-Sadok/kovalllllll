using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Commands.ProductCommands;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using NSubstitute;
using Xunit;

namespace DroneBuilder.Application.Tests.ProductCommandTests;

public class RemovePropertyFromProductCommandHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly RemovePropertyFromProductCommandHandler _handler;

    private static readonly Guid ProductId = Guid.NewGuid();
    private static readonly Guid PropertyId = Guid.NewGuid();

    public RemovePropertyFromProductCommandHandlerTests()
    {
        _productRepository = Substitute.For<IProductRepository>();
        _handler = new RemovePropertyFromProductCommandHandler(_productRepository);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenProductExists_ShouldRemovePropertyValues()
    {
        // Arrange
        var command = new RemovePropertyFromProductCommand(ProductId, PropertyId);
        var ppv = new ProductPropertyValue { ProductId = ProductId, PropertyId = PropertyId, ValueId = Guid.NewGuid() };
        var product = new Product
        {
            Id = ProductId,
            ProductPropertyValues = new List<ProductPropertyValue> { ppv }
        };

        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>()).Returns(product);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Empty(product.ProductPropertyValues);
        await _productRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new RemovePropertyFromProductCommand(ProductId, PropertyId);
        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>()).Returns((Product)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.ExecuteCommandAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenPropertyNotOnProduct_ShouldDoNothing()
    {
        // Arrange
        var command = new RemovePropertyFromProductCommand(ProductId, PropertyId);
        var otherPropertyId = Guid.NewGuid();
        var ppv = new ProductPropertyValue { ProductId = ProductId, PropertyId = otherPropertyId, ValueId = Guid.NewGuid() };
        var product = new Product
        {
            Id = ProductId,
            ProductPropertyValues = new List<ProductPropertyValue> { ppv }
        };

        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>()).Returns(product);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Single(product.ProductPropertyValues);
        await _productRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
