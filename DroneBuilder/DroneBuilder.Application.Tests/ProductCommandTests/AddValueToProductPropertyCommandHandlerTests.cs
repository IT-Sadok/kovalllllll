using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Commands.ProductCommands;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using NSubstitute;
using Xunit;

namespace DroneBuilder.Application.Tests.ProductCommandTests;

public class AddValueToProductPropertyCommandHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly IPropertyRepository _propertyRepository;
    private readonly IValueRepository _valueRepository;
    private readonly AddValueToProductPropertyCommandHandler _handler;

    private static readonly Guid ProductId = Guid.NewGuid();
    private static readonly Guid PropertyId = Guid.NewGuid();
    private static readonly Guid ValueId = Guid.NewGuid();

    public AddValueToProductPropertyCommandHandlerTests()
    {
        _productRepository = Substitute.For<IProductRepository>();
        _propertyRepository = Substitute.For<IPropertyRepository>();
        _valueRepository = Substitute.For<IValueRepository>();

        _handler = new AddValueToProductPropertyCommandHandler(
            _productRepository,
            _propertyRepository,
            _valueRepository);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenAllExist_ShouldAddValueSuccessfully()
    {
        // Arrange
        var command = new AddValueToProductPropertyCommand(ProductId, PropertyId, ValueId);

        var product = new Product { Id = ProductId, ProductPropertyValues = new List<ProductPropertyValue>() };
        var property = new Property { Id = PropertyId };
        var value = new Value { Id = ValueId };

        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(product);
        _propertyRepository.GetPropertyByIdAsync(PropertyId, Arg.Any<CancellationToken>())
            .Returns(property);
        _valueRepository.GetValueByIdAsync(ValueId, Arg.Any<CancellationToken>())
            .Returns(value);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(product.ProductPropertyValues);
        Assert.Single(product.ProductPropertyValues);
        var ppv = product.ProductPropertyValues.First();
        Assert.Equal(ProductId, ppv.ProductId);
        Assert.Equal(PropertyId, ppv.PropertyId);
        Assert.Equal(ValueId, ppv.ValueId);

        await _productRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new AddValueToProductPropertyCommand(ProductId, PropertyId, ValueId);
        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>()).Returns((Product)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.ExecuteCommandAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenPropertyNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new AddValueToProductPropertyCommand(ProductId, PropertyId, ValueId);
        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>()).Returns(new Product());
        _propertyRepository.GetPropertyByIdAsync(PropertyId, Arg.Any<CancellationToken>()).Returns((Property)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.ExecuteCommandAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenValueNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new AddValueToProductPropertyCommand(ProductId, PropertyId, ValueId);
        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>()).Returns(new Product());
        _propertyRepository.GetPropertyByIdAsync(PropertyId, Arg.Any<CancellationToken>()).Returns(new Property());
        _valueRepository.GetValueByIdAsync(ValueId, Arg.Any<CancellationToken>()).Returns((Value)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.ExecuteCommandAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenAlreadyAssociated_ShouldThrowValidationException()
    {
        // Arrange
        var command = new AddValueToProductPropertyCommand(ProductId, PropertyId, ValueId);
        var product = new Product
        {
            Id = ProductId,
            ProductPropertyValues = new List<ProductPropertyValue>
            {
                new ProductPropertyValue { ProductId = ProductId, PropertyId = PropertyId, ValueId = ValueId }
            }
        };

        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>()).Returns(product);
        _propertyRepository.GetPropertyByIdAsync(PropertyId, Arg.Any<CancellationToken>()).Returns(new Property { Id = PropertyId });
        _valueRepository.GetValueByIdAsync(ValueId, Arg.Any<CancellationToken>()).Returns(new Value { Id = ValueId });

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.ExecuteCommandAsync(command, CancellationToken.None));
    }
}
