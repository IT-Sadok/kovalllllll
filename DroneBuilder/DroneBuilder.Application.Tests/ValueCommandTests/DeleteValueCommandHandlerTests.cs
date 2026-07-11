using DroneBuilder.Application.Mediator.Commands.ValueCommands;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using NSubstitute;

namespace DroneBuilder.Application.Tests.ValueCommandTests;

public class DeleteValueCommandHandlerTests
{
    private readonly IValueRepository _valueRepository;
    private readonly DeleteValueCommandHandler _handler;

    private static readonly Guid ValueId = Guid.NewGuid();

    public DeleteValueCommandHandlerTests()
    {
        _valueRepository = Substitute.For<IValueRepository>();
        _handler = new DeleteValueCommandHandler(_valueRepository);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenValueExists_ShouldRemoveValueAndSaveChanges()
    {
        // Arrange
        var command = new DeleteValueCommand(ValueId);
        var value = new Value { Id = ValueId };

        _valueRepository.GetValueByIdAsync(Arg.Is<Guid>(id => id == ValueId), Arg.Any<CancellationToken>())
            .Returns(value);

        // Act
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        _valueRepository.Received(1).RemoveValue(Arg.Is<Value>(v => v.Id == ValueId));
        await _valueRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenValueDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        var command = new DeleteValueCommand(ValueId);

        _valueRepository.GetValueByIdAsync(Arg.Is<Guid>(id => id == ValueId), Arg.Any<CancellationToken>())
            .Returns((Value)null);

        // Act & Assert
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<NotFoundError>());
        Assert.Equal($"Value with id {ValueId} not found.", result.Errors[0].Message);

        _valueRepository.DidNotReceive().RemoveValue(Arg.Any<Value>());
        await _valueRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
