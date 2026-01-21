using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Commands.PropertyCommands;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using MapsterMapper;
using NSubstitute;
using Xunit;

namespace DroneBuilder.Application.Tests.PropertyCommandTests;

public class UpdatePropertyCommandHandlerTests
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IMapper _mapper;
    private readonly UpdatePropertyCommandHandler _handler;

    private static readonly Guid PropertyId = Guid.NewGuid();
    private const string OriginalName = "Original Property";
    private const string UpdatedName = "Updated Property";

    public UpdatePropertyCommandHandlerTests()
    {
        // Arrange
        _propertyRepository = Substitute.For<IPropertyRepository>();
        _mapper = Substitute.For<IMapper>();

        _handler = new UpdatePropertyCommandHandler(
            _propertyRepository,
            _mapper);
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

        _mapper.Map<PropertyModel>(Arg.Is<Property>(p =>
                p.Id == PropertyId &&
                p.Name == UpdatedName))
            .Returns(expectedPropertyModel);

        // Act
        var result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(UpdatedName, result.Name);
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
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal($"Property with id {PropertyId} not found.", exception.Message);

        await _propertyRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());

        _mapper.DidNotReceive().Map<PropertyModel>(Arg.Any<Property>());
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

        _mapper.Map<PropertyModel>(Arg.Any<Property>())
            .Returns(new PropertyModel());

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(OriginalName, existingProperty.Name);

        await _propertyRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}