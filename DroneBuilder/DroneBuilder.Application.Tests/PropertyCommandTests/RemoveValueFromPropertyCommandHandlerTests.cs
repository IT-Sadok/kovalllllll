using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Commands.PropertyCommands;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using NSubstitute;
using Xunit;

namespace DroneBuilder.Application.Tests.PropertyCommandTests;

public class RemoveValueFromPropertyCommandHandlerTests
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IValueRepository _valueRepository;
    private readonly RemoveValueFromPropertyCommandHandler _handler;

    private static readonly Guid PropertyId = Guid.NewGuid();
    private static readonly Guid ValueId = Guid.NewGuid();

    public RemoveValueFromPropertyCommandHandlerTests()
    {
        _propertyRepository = Substitute.For<IPropertyRepository>();
        _valueRepository = Substitute.For<IValueRepository>();
        _handler = new RemoveValueFromPropertyCommandHandler(_propertyRepository, _valueRepository);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenBothExistAndLinked_ShouldRemoveLink()
    {
        // Arrange
        var command = new RemoveValueFromPropertyCommand(PropertyId, ValueId);
        var property = new Property { Id = PropertyId, Values = new List<Value>() };
        var value = new Value { Id = ValueId, Properties = new List<Property> { property } };
        property.Values.Add(value);

        _propertyRepository.GetPropertyByIdAsync(PropertyId, Arg.Any<CancellationToken>()).Returns(property);
        _valueRepository.GetValueByIdAsync(ValueId, Arg.Any<CancellationToken>()).Returns(value);
        _valueRepository.GetValueWithPropertiesByIdAsync(ValueId, Arg.Any<CancellationToken>()).Returns(value);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.DoesNotContain(value, property.Values);
        await _propertyRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenValueBecomesOrphan_ShouldDeleteValue()
    {
        // Arrange
        var command = new RemoveValueFromPropertyCommand(PropertyId, ValueId);
        var property = new Property { Id = PropertyId, Values = new List<Value>() };
        var value = new Value { Id = ValueId, Properties = new List<Property>() }; // Orphan after removal
        property.Values.Add(value);

        _propertyRepository.GetPropertyByIdAsync(PropertyId, Arg.Any<CancellationToken>()).Returns(property);
        _valueRepository.GetValueByIdAsync(ValueId, Arg.Any<CancellationToken>()).Returns(value);
        _valueRepository.GetValueWithPropertiesByIdAsync(ValueId, Arg.Any<CancellationToken>()).Returns(value);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        _valueRepository.Received(1).RemoveValue(value);
        await _valueRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenPropertyNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new RemoveValueFromPropertyCommand(PropertyId, ValueId);
        _propertyRepository.GetPropertyByIdAsync(PropertyId, Arg.Any<CancellationToken>()).Returns((Property)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.ExecuteCommandAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenValueNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new RemoveValueFromPropertyCommand(PropertyId, ValueId);
        _propertyRepository.GetPropertyByIdAsync(PropertyId, Arg.Any<CancellationToken>()).Returns(new Property());
        _valueRepository.GetValueByIdAsync(ValueId, Arg.Any<CancellationToken>()).Returns((Value)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.ExecuteCommandAsync(command, CancellationToken.None));
    }
}
