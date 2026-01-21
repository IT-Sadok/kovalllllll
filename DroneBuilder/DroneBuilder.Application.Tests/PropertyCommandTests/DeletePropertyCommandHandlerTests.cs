using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Commands.PropertyCommands;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using NSubstitute;

namespace DroneBuilder.Application.Tests.PropertyCommandTests;

public class DeletePropertyCommandHandlerTests
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly DeletePropertyCommandHandler _handler;

    private static readonly Guid PropertyId = Guid.NewGuid();
    private const string PropertyName = "Test Property";

    public DeletePropertyCommandHandlerTests()
    {
        // Arrange
        _propertyRepository = Substitute.For<IPropertyRepository>();

        _handler = new DeletePropertyCommandHandler(_propertyRepository);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenPropertyExists_ShouldDeleteProperty()
    {
        // Arrange
        var command = new DeletePropertyCommand(PropertyId);

        var existingProperty = new Property
        {
            Id = PropertyId,
            Name = PropertyName
        };

        _propertyRepository.GetPropertyByIdAsync(
                Arg.Is<Guid>(id => id == PropertyId),
                Arg.Any<CancellationToken>())
            .Returns(existingProperty);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        _propertyRepository.Received(1).RemoveProperty(
            Arg.Is<Property>(p => p.Id == PropertyId));

        await _propertyRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenPropertyNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new DeletePropertyCommand(PropertyId);

        _propertyRepository.GetPropertyByIdAsync(
                Arg.Is<Guid>(id => id == PropertyId),
                Arg.Any<CancellationToken>())
            .Returns((Property)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal($"Property with id {PropertyId} not found.", exception.Message);

        _propertyRepository.DidNotReceive().RemoveProperty(Arg.Any<Property>());

        await _propertyRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenSuccessful_ShouldRemoveExactProperty()
    {
        // Arrange
        var command = new DeletePropertyCommand(PropertyId);

        var existingProperty = new Property
        {
            Id = PropertyId,
            Name = PropertyName
        };

        _propertyRepository.GetPropertyByIdAsync(
                Arg.Is<Guid>(id => id == PropertyId),
                Arg.Any<CancellationToken>())
            .Returns(existingProperty);

        Property removedProperty = null;
        _propertyRepository.When(x => x.RemoveProperty(Arg.Is<Property>(p => p.Id == PropertyId)))
            .Do(callInfo => removedProperty = callInfo.Arg<Property>());

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(removedProperty);
        Assert.Same(existingProperty, removedProperty);
    }
}