using DroneBuilder.Application.Mediator.Commands.ValueCommands;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using MapsterMapper;
using NSubstitute;

namespace DroneBuilder.Application.Tests.ValueCommandTests;

public class CreateValueCommandHandlerTests
{
    private readonly IValueRepository _valueRepository;
    private readonly IMapper _mapper;
    private readonly CreateValueCommandHandler _handler;

    private static readonly Guid ValueId = Guid.NewGuid();
    private const string TextValue = "Test Value";

    public CreateValueCommandHandlerTests()
    {
        // Arrange
        _valueRepository = Substitute.For<IValueRepository>();
        _mapper = Substitute.For<IMapper>();

        _handler = new CreateValueCommandHandler(
            _valueRepository,
            _mapper);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenValidModel_ShouldCreateValueSuccessfully()
    {
        // Arrange
        var createValueModel = new CreateValueModel
        {
            Text = TextValue
        };
        var command = new CreateValueCommand(createValueModel);

        var mappedValue = new Value
        {
            Id = ValueId,
            Text = TextValue
        };

        var expectedValueModel = new ValueModel
        {
            Id = ValueId,
            Text = TextValue
        };

        _mapper.Map<Value>(Arg.Is<CreateValueModel>(m => m.Text == TextValue))
            .Returns(mappedValue);

        _mapper.Map<ValueModel>(Arg.Is<Value>(v =>
                v.Id == ValueId &&
                v.Text == TextValue))
            .Returns(expectedValueModel);

        // Act
        var result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ValueId, result.Id);
        Assert.Equal(TextValue, result.Text);

        await _valueRepository.Received(1).AddValueAsync(
            Arg.Is<Value>(v => v.Text == TextValue),
            Arg.Any<CancellationToken>());

        await _valueRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        _mapper.Received(1).Map<Value>(Arg.Is<CreateValueModel>(m => m.Text == TextValue));

        _mapper.Received(1).Map<ValueModel>(Arg.Is<Value>(v => v.Id == ValueId));
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenCalled_ShouldMapModelToEntity()
    {
        // Arrange
        var createValueModel = new CreateValueModel
        {
            Text = TextValue
        };
        var command = new CreateValueCommand(createValueModel);

        var mappedValue = new Value { Id = ValueId, Text = TextValue };

        _mapper.Map<Value>(Arg.Is<CreateValueModel>(m => m.Text == TextValue))
            .Returns(mappedValue);

        _mapper.Map<ValueModel>(Arg.Is<Value>(v => v.Text == TextValue))
            .Returns(new ValueModel());

        Value capturedValue = null;
        await _valueRepository.AddValueAsync(
            Arg.Do<Value>(v => capturedValue = v),
            Arg.Any<CancellationToken>());

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedValue);
        Assert.Equal(TextValue, capturedValue.Text);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenCalled_ShouldReturnMappedModel()
    {
        // Arrange
        var createValueModel = new CreateValueModel
        {
            Text = TextValue
        };
        var command = new CreateValueCommand(createValueModel);

        var mappedValue = new Value { Id = ValueId };
        var expectedModel = new ValueModel { Id = ValueId, Text = TextValue };

        _mapper.Map<Value>(Arg.Any<CreateValueModel>())
            .Returns(mappedValue);

        _mapper.Map<ValueModel>(Arg.Is<Value>(v => v.Id == ValueId))
            .Returns(expectedModel);

        // Act
        var result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Same(expectedModel, result);
    }
}