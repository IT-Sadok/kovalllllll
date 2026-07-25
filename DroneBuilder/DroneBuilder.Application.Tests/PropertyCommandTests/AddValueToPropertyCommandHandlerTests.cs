using DroneBuilder.Application.Features.Catalog.Properties.AddValueToProperty;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
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
            .Returns((Property?)null);

        // Act & Assert
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<NotFoundError>());

        Assert.Equal($"Property with ID {PropertyId} not found.", result.Errors[0].Message);

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
            .Returns((Value?)null);

        // Act & Assert
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<NotFoundError>());

        Assert.Equal($"Value with ID {ValueId} not found.", result.Errors[0].Message);

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
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<ValidationError>());

        Assert.Contains($"Value with ID {ValueId} is already associated with Property ID {PropertyId}",
            result.Errors[0].Message);

        Assert.Single(property.Values);

        await _propertyRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
