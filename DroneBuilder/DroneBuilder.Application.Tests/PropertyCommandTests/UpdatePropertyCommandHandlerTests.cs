using DroneBuilder.Application.Mediator.Commands.PropertyCommands;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using NSubstitute;

namespace DroneBuilder.Application.Tests.PropertyCommandTests;

public class UpdatePropertyCommandHandlerTests
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly UpdatePropertyCommandHandler _handler;

    private static readonly Guid PropertyId = Guid.NewGuid();
    private const string OriginalName = "Original Property";
    private const string UpdatedName = "Updated Property";

    public UpdatePropertyCommandHandlerTests()
    {
        // Arrange
        _propertyRepository = Substitute.For<IPropertyRepository>();

        _handler = new UpdatePropertyCommandHandler(
            _propertyRepository);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenNameProvided_ShouldUpdateName()
    {
        // Arrange
        var updateModel = new UpdatePropertyModel
        {
            Name = UpdatedName
        };
        var command = new UpdatePropertyCommand(PropertyId, updateModel);

        var existingProperty = new Property
        {
            Id = PropertyId,
            Name = OriginalName
        };

        var expectedPropertyModel = new PropertyModel
        {
            Id = PropertyId,
            Name = UpdatedName
        };

        _propertyRepository.GetPropertyByIdAsync(
                Arg.Is<Guid>(id => id == PropertyId),
                Arg.Any<CancellationToken>())
            .Returns(existingProperty);

        // Act
        Result<PropertyModel> result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(UpdatedName, result.Value.Name);
        Assert.Equal(UpdatedName, existingProperty.Name);

        await _propertyRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenPropertyNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var updateModel = new UpdatePropertyModel
        {
            Name = UpdatedName
        };
        var command = new UpdatePropertyCommand(PropertyId, updateModel);

        _propertyRepository.GetPropertyByIdAsync(
                Arg.Is<Guid>(id => id == PropertyId),
                Arg.Any<CancellationToken>())
            .Returns((Property)null);

        // Act & Assert
        Result<PropertyModel> result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<NotFoundError>());

        Assert.Equal($"Property with id {PropertyId} not found.", result.Errors[0].Message);

        await _propertyRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());

    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenNameIsNull_ShouldNotUpdateName()
    {
        // Arrange
        var updateModel = new UpdatePropertyModel
        {
            Name = null
        };
        var command = new UpdatePropertyCommand(PropertyId, updateModel);

        var existingProperty = new Property
        {
            Id = PropertyId,
            Name = OriginalName
        };

        _propertyRepository.GetPropertyByIdAsync(
                Arg.Is<Guid>(id => id == PropertyId),
                Arg.Any<CancellationToken>())
            .Returns(existingProperty);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(OriginalName, existingProperty.Name);

        await _propertyRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

