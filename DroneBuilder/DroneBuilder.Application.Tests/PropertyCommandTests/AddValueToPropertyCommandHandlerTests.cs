using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Commands.PropertyCommands;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using NSubstitute;

namespace DroneBuilder.Application.Tests.PropertyCommandTests;

public class AddValueToPropertyCommandHandlerTests
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IValueRepository _valueRepository;
    private readonly AddValueToPropertyCommandHandler _handler;

    private static readonly Guid PropertyId = Guid.NewGuid();
    private static readonly Guid ValueId = Guid.NewGuid();

    public AddValueToPropertyCommandHandlerTests()
    {
        // Arrange
        _propertyRepository = Substitute.For<IPropertyRepository>();
        _valueRepository = Substitute.For<IValueRepository>();

        _handler = new AddValueToPropertyCommandHandler(
            _propertyRepository,
            _valueRepository);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenPropertyAndValueExist_ShouldAddValueSuccessfully()
    {
        // Arrange
        var command = new AddValueToPropertyCommand(PropertyId, ValueId);

        var property = new Property
        {
            Id = PropertyId,
            Values = new List<Value>()
        };

        var value = new Value
        {
            Id = ValueId
        };

        _propertyRepository.GetPropertyByIdAsync(
                Arg.Is<Guid>(id => id == PropertyId),
                Arg.Any<CancellationToken>())
            .Returns(property);

        _valueRepository.GetValueByIdAsync(
                Arg.Is<Guid>(id => id == ValueId),
                Arg.Any<CancellationToken>())
            .Returns(value);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(property.Values);
        Assert.Contains(value, property.Values);

        await _propertyRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenPropertyNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new AddValueToPropertyCommand(PropertyId, ValueId);

        _propertyRepository.GetPropertyByIdAsync(
                Arg.Is<Guid>(id => id == PropertyId),
                Arg.Any<CancellationToken>())
            .Returns((Property)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal($"Property with ID {PropertyId} not found.", exception.Message);

        await _valueRepository.DidNotReceive().GetValueByIdAsync(
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>());

        await _propertyRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenValueNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new AddValueToPropertyCommand(PropertyId, ValueId);

        var property = new Property
        {
            Id = PropertyId,
            Values = new List<Value>()
        };

        _propertyRepository.GetPropertyByIdAsync(
                Arg.Is<Guid>(id => id == PropertyId),
                Arg.Any<CancellationToken>())
            .Returns(property);

        _valueRepository.GetValueByIdAsync(
                Arg.Is<Guid>(id => id == ValueId),
                Arg.Any<CancellationToken>())
            .Returns((Value)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal($"Value with ID {ValueId} not found.", exception.Message);

        await _propertyRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenValueAlreadyExists_ShouldThrowValidationException()
    {
        // Arrange
        var command = new AddValueToPropertyCommand(PropertyId, ValueId);

        var value = new Value
        {
            Id = ValueId
        };

        var property = new Property
        {
            Id = PropertyId,
            Values = new List<Value> { value }
        };

        _propertyRepository.GetPropertyByIdAsync(
                Arg.Is<Guid>(id => id == PropertyId),
                Arg.Any<CancellationToken>())
            .Returns(property);

        _valueRepository.GetValueByIdAsync(
                Arg.Is<Guid>(id => id == ValueId),
                Arg.Any<CancellationToken>())
            .Returns(value);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));


        Assert.Contains($"Value with ID {ValueId} is already associated with Property ID {PropertyId}",
            exception.Message);

        Assert.Single(property.Values);

        await _propertyRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}