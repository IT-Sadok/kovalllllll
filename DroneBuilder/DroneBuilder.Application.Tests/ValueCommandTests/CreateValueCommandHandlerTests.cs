using DroneBuilder.Application.Mediator.Commands.ValueCommands;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using MapsterMapper;
using NSubstitute;
using Xunit;

namespace DroneBuilder.Application.Tests.ValueCommandTests;

public class CreateValueCommandHandlerTests
{
    private readonly IValueRepository _valueRepository;
    private readonly IPropertyRepository _propertyRepository;
    private readonly IMapper _mapper;
    private readonly CreateValueCommandHandler _handler;

    private static readonly Guid ValueId = Guid.NewGuid();
    private static readonly Guid PropertyId = Guid.NewGuid();
    private const string TextValue = "Test Value";

    public CreateValueCommandHandlerTests()
    {
        // Arrange
        _valueRepository = Substitute.For<IValueRepository>();
        _propertyRepository = Substitute.For<IPropertyRepository>();
        _mapper = Substitute.For<IMapper>();

        _handler = new CreateValueCommandHandler(
            _valueRepository,
            _propertyRepository,
            _mapper);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenValidModel_ShouldCreateValueSuccessfully()
    {
        // Arrange
        var createValueModel = new CreateValueModel
        {
            PropertyId = PropertyId,
            Text = TextValue
        };
        var command = new CreateValueCommand(createValueModel);

        var property = new Property { Id = PropertyId, Values = new List<Value>() };
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

        _propertyRepository.GetPropertyByIdAsync(PropertyId, Arg.Any<CancellationToken>())
            .Returns(property);

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
        Assert.Contains(mappedValue, property.Values);

        await _valueRepository.Received(1).AddValueAsync(
            Arg.Is<Value>(v => v.Text == TextValue),
            Arg.Any<CancellationToken>());

        await _valueRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenCalled_ShouldMapModelToEntity()
    {
        // Arrange
        var createValueModel = new CreateValueModel
        {
            PropertyId = PropertyId,
            Text = TextValue
        };
        var command = new CreateValueCommand(createValueModel);

        var property = new Property { Id = PropertyId, Values = new List<Value>() };
        var mappedValue = new Value { Id = ValueId, Text = TextValue };

        _propertyRepository.GetPropertyByIdAsync(PropertyId, Arg.Any<CancellationToken>())
            .Returns(property);

        _mapper.Map<Value>(Arg.Is<CreateValueModel>(m => m.Text == TextValue))
            .Returns(mappedValue);

        _mapper.Map<ValueModel>(Arg.Is<Value>(v => v.Text == TextValue))
            .Returns(new ValueModel());

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        await _valueRepository.Received(1).AddValueAsync(
            Arg.Is<Value>(v => v.Text == TextValue),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenCalled_ShouldReturnMappedModel()
    {
        // Arrange
        var createValueModel = new CreateValueModel
        {
            PropertyId = PropertyId,
            Text = TextValue
        };
        var command = new CreateValueCommand(createValueModel);

        var property = new Property { Id = PropertyId, Values = new List<Value>() };
        var mappedValue = new Value { Id = ValueId };
        var expectedModel = new ValueModel { Id = ValueId, Text = TextValue };

        _propertyRepository.GetPropertyByIdAsync(PropertyId, Arg.Any<CancellationToken>())
            .Returns(property);

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