using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Commands.ProductCommands;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using NSubstitute;

namespace DroneBuilder.Application.Tests.ProductCommandTests;

public class AddPropertyToProductCommandHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly IPropertyRepository _propertyRepository;
    private readonly AddPropertyToProductCommandHandler _handler;

    private static readonly Guid ProductId = Guid.NewGuid();
    private static readonly Guid PropertyId = Guid.NewGuid();

    public AddPropertyToProductCommandHandlerTests()
    {
        // Arrange
        _productRepository = Substitute.For<IProductRepository>();
        _propertyRepository = Substitute.For<IPropertyRepository>();

        _handler = new AddPropertyToProductCommandHandler(
            _productRepository,
            _propertyRepository);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenProductAndPropertyExist_ShouldAddPropertySuccessfully()
    {
        // Arrange
        var command = new AddPropertyToProductCommand(ProductId, PropertyId);

        var product = new Product
        {
            Id = ProductId,
            Properties = new List<Property>()
        };

        var property = new Property
        {
            Id = PropertyId
        };

        _productRepository.GetProductByIdAsync(
                Arg.Is<Guid>(id => id == ProductId),
                Arg.Any<CancellationToken>())
            .Returns(product);

        _propertyRepository.GetPropertyByIdAsync(
                Arg.Is<Guid>(id => id == PropertyId),
                Arg.Any<CancellationToken>())
            .Returns(property);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(product.Properties);
        Assert.Contains(property, product.Properties);

        await _productRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new AddPropertyToProductCommand(ProductId, PropertyId);

        _productRepository.GetProductByIdAsync(
                Arg.Is<Guid>(id => id == ProductId),
                Arg.Any<CancellationToken>())
            .Returns((Product)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal($"Product with ID {ProductId} not found.", exception.Message);

        await _propertyRepository.DidNotReceive().GetPropertyByIdAsync(
            Arg.Is<Guid>(id => id == PropertyId),
            Arg.Any<CancellationToken>());

        await _productRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenPropertyNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new AddPropertyToProductCommand(ProductId, PropertyId);

        var product = new Product
        {
            Id = ProductId,
            Properties = new List<Property>()
        };

        _productRepository.GetProductByIdAsync(
                Arg.Is<Guid>(id => id == ProductId),
                Arg.Any<CancellationToken>())
            .Returns(product);

        _propertyRepository.GetPropertyByIdAsync(
                Arg.Is<Guid>(id => id == PropertyId),
                Arg.Any<CancellationToken>())
            .Returns((Property)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal($"Property with ID {PropertyId} not found.", exception.Message);

        await _productRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenPropertyAlreadyExists_ShouldThrowValidationException()
    {
        // Arrange
        var command = new AddPropertyToProductCommand(ProductId, PropertyId);

        var property = new Property
        {
            Id = PropertyId
        };

        var product = new Product
        {
            Id = ProductId,
            Properties = new List<Property> { property }
        };

        _productRepository.GetProductByIdAsync(
                Arg.Is<Guid>(id => id == ProductId),
                Arg.Any<CancellationToken>())
            .Returns(product);

        _propertyRepository.GetPropertyByIdAsync(
                Arg.Is<Guid>(id => id == PropertyId),
                Arg.Any<CancellationToken>())
            .Returns(property);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Contains($"Property with ID {PropertyId} is already associated with Product ID {ProductId}",
            exception.Message);

        Assert.Single(product.Properties);

        await _productRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}