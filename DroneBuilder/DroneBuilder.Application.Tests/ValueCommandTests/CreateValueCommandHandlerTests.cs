using DroneBuilder.Application.Features.Catalog.Values.CreateValue;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;
using NSubstitute;

namespace DroneBuilder.Application.Tests.ValueCommandTests;

public class CreateValueCommandHandlerTests
{
    private readonly IValueRepository _valueRepository;
    private readonly IPropertyRepository _propertyRepository;
    private readonly CreateValueCommandHandler _handler;

    private static readonly Guid ValueId = Guid.NewGuid();
    private static readonly Guid PropertyId = Guid.NewGuid();
    private const string TextValue = "Test Value";

    public CreateValueCommandHandlerTests()
    {
        // Arrange
        _valueRepository = Substitute.For<IValueRepository>();
        _propertyRepository = Substitute.For<IPropertyRepository>();

        _handler = new CreateValueCommandHandler(
            _valueRepository,
            _propertyRepository);
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

        // Act
        Result<ValueModel> result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(TextValue, result.Value.Text);
        Assert.Contains(property.Values, v => v.Text == TextValue);

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

        // Act
        Result<ValueModel> result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(TextValue, result.Value.Text);
    }
}

