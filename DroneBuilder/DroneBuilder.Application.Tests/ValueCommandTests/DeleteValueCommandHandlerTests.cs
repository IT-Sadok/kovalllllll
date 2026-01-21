using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Commands.ValueCommands;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using NSubstitute;

namespace DroneBuilder.Application.Tests.ValueCommandTests;

public class DeleteValueCommandHandlerTests
{
    private readonly IValueRepository _valueRepository;
    private readonly DeleteValueCommandHandler _handler;

    private static readonly Guid ValueId = Guid.NewGuid();
    private const string TextValue = "Test Value";

    public DeleteValueCommandHandlerTests()
    {
        // Arrange
        _valueRepository = Substitute.For<IValueRepository>();

        _handler = new DeleteValueCommandHandler(_valueRepository);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenValueExists_ShouldDeleteValue()
    {
        // Arrange
        var command = new DeleteValueCommand(ValueId);

        var existingValue = new Value
        {
            Id = ValueId,
            Text = TextValue
        };

        _valueRepository.GetValueByIdAsync(
                Arg.Is<Guid>(id => id == ValueId),
                Arg.Any<CancellationToken>())
            .Returns(existingValue);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        _valueRepository.Received(1).RemoveValue(
            Arg.Is<Value>(v => v.Id == ValueId));

        await _valueRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenValueNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new DeleteValueCommand(ValueId);

        _valueRepository.GetValueByIdAsync(
                Arg.Is<Guid>(id => id == ValueId),
                Arg.Any<CancellationToken>())
            .Returns((Value)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal($"Value with id {ValueId} not found.", exception.Message);

        _valueRepository.DidNotReceive().RemoveValue(Arg.Any<Value>());

        await _valueRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenSuccessful_ShouldRemoveExactValue()
    {
        // Arrange
        var command = new DeleteValueCommand(ValueId);

        var existingValue = new Value
        {
            Id = ValueId,
            Text = TextValue
        };

        _valueRepository.GetValueByIdAsync(
                Arg.Is<Guid>(id => id == ValueId),
                Arg.Any<CancellationToken>())
            .Returns(existingValue);

        Value removedValue = null;
        _valueRepository.When(x => x.RemoveValue(Arg.Is<Value>(v => v.Id == ValueId)))
            .Do(callInfo => removedValue = callInfo.Arg<Value>());

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(removedValue);
        Assert.Same(existingValue, removedValue);
    }
}