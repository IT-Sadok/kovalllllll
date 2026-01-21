using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Commands.ValueCommands;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using MapsterMapper;
using NSubstitute;

namespace DroneBuilder.Application.Tests.ValueCommandTests;

public class UpdateValueCommandHandlerTests
{
    private readonly IValueRepository _valueRepository;
    private readonly IMapper _mapper;
    private readonly UpdateValueCommandHandler _handler;

    private static readonly Guid ValueId = Guid.NewGuid();
    private const string OriginalText = "Original Text";
    private const string UpdatedText = "Updated Text";

    public UpdateValueCommandHandlerTests()
    {
        // Arrange
        _valueRepository = Substitute.For<IValueRepository>();
        _mapper = Substitute.For<IMapper>();

        _handler = new UpdateValueCommandHandler(
            _valueRepository,
            _mapper);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenTextProvided_ShouldUpdateText()
    {
        // Arrange
        var updateModel = new UpdateValueModel
        {
            Text = UpdatedText
        };
        var command = new UpdateValueCommand(ValueId, updateModel);

        var existingValue = new Value
        {
            Id = ValueId,
            Text = OriginalText
        };

        var expectedValueModel = new ValueModel
        {
            Id = ValueId,
            Text = UpdatedText
        };

        _valueRepository.GetValueByIdAsync(
                Arg.Is<Guid>(id => id == ValueId),
                Arg.Any<CancellationToken>())
            .Returns(existingValue);

        _mapper.Map<ValueModel>(Arg.Is<Value>(v =>
                v.Id == ValueId &&
                v.Text == UpdatedText))
            .Returns(expectedValueModel);

        // Act
        var result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(UpdatedText, result.Text);
        Assert.Equal(UpdatedText, existingValue.Text);

        await _valueRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenValueNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var updateModel = new UpdateValueModel
        {
            Text = UpdatedText
        };
        var command = new UpdateValueCommand(ValueId, updateModel);

        _valueRepository.GetValueByIdAsync(
                Arg.Is<Guid>(id => id == ValueId),
                Arg.Any<CancellationToken>())
            .Returns((Value)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal($"Value with id {ValueId} not found.", exception.Message);

        await _valueRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());

        _mapper.DidNotReceive().Map<ValueModel>(Arg.Is<Value>(v => v.Text == UpdatedText));
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenTextIsNull_ShouldNotUpdateText()
    {
        // Arrange
        var updateModel = new UpdateValueModel
        {
            Text = null
        };
        var command = new UpdateValueCommand(ValueId, updateModel);

        var existingValue = new Value
        {
            Id = ValueId,
            Text = OriginalText
        };

        _valueRepository.GetValueByIdAsync(
                Arg.Is<Guid>(id => id == ValueId),
                Arg.Any<CancellationToken>())
            .Returns(existingValue);

        _mapper.Map<ValueModel>(Arg.Is<Value>(v => v.Text == OriginalText))
            .Returns(new ValueModel());

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(OriginalText, existingValue.Text);

        await _valueRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}