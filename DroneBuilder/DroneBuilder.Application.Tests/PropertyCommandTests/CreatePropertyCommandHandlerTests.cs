using DroneBuilder.Application.Mediator.Commands.PropertyCommands;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using MapsterMapper;
using NSubstitute;
using Xunit;

namespace DroneBuilder.Application.Tests.PropertyCommandTests;

public class CreatePropertyCommandHandlerTests
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IMapper _mapper;
    private readonly CreatePropertyCommandHandler _handler;

    private static readonly Guid PropertyId = Guid.NewGuid();
    private const string PropertyName = "Test Property";

    public CreatePropertyCommandHandlerTests()
    {
        // Arrange
        _propertyRepository = Substitute.For<IPropertyRepository>();
        _mapper = Substitute.For<IMapper>();

        _handler = new CreatePropertyCommandHandler(
            _propertyRepository,
            _mapper);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenValidModel_ShouldCreatePropertySuccessfully()
    {
        // Arrange
        var createPropertyModel = new CreatePropertyModel
        {
            Name = PropertyName
        };
        var command = new CreatePropertyCommand(createPropertyModel);

        var mappedProperty = new Property
        {
            Id = PropertyId,
            Name = PropertyName
        };

        var expectedPropertyModel = new PropertyModel
        {
            Id = PropertyId,
            Name = PropertyName
        };

        _mapper.Map<Property>(Arg.Is<CreatePropertyModel>(m => m.Name == PropertyName))
            .Returns(mappedProperty);

        _mapper.Map<PropertyModel>(Arg.Is<Property>(p =>
                p.Id == PropertyId &&
                p.Name == PropertyName))
            .Returns(expectedPropertyModel);

        // Act
        var result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PropertyId, result.Id);
        Assert.Equal(PropertyName, result.Name);

        await _propertyRepository.Received(1).AddPropertyAsync(
            Arg.Is<Property>(p => p.Name == PropertyName),
            Arg.Any<CancellationToken>());

        await _propertyRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        _mapper.Received(1).Map<Property>(Arg.Is<CreatePropertyModel>(m => m.Name == PropertyName));

        _mapper.Received(1).Map<PropertyModel>(Arg.Is<Property>(p => p.Id == PropertyId));
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenCalled_ShouldMapModelToEntity()
    {
        // Arrange
        var createPropertyModel = new CreatePropertyModel
        {
            Name = PropertyName
        };
        var command = new CreatePropertyCommand(createPropertyModel);

        var mappedProperty = new Property { Id = PropertyId, Name = PropertyName };

        _mapper.Map<Property>(Arg.Is<CreatePropertyModel>(m => m.Name == PropertyName))
            .Returns(mappedProperty);

        _mapper.Map<PropertyModel>(Arg.Is<Property>(p => p.Name == PropertyName))
            .Returns(new PropertyModel());

        Property capturedProperty = null;
        await _propertyRepository.AddPropertyAsync(
            Arg.Do<Property>(p => capturedProperty = p),
            Arg.Any<CancellationToken>());

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedProperty);
        Assert.Equal(PropertyName, capturedProperty.Name);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenCalled_ShouldReturnMappedModel()
    {
        // Arrange
        var createPropertyModel = new CreatePropertyModel
        {
            Name = PropertyName
        };
        var command = new CreatePropertyCommand(createPropertyModel);

        var mappedProperty = new Property { Id = PropertyId };
        var expectedModel = new PropertyModel { Id = PropertyId, Name = PropertyName };

        _mapper.Map<Property>(Arg.Is<CreatePropertyModel>(m => m.Name == PropertyName))
            .Returns(mappedProperty);

        _mapper.Map<PropertyModel>(Arg.Is<Property>(p => p.Id == PropertyId))
            .Returns(expectedModel);

        // Act
        var result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Same(expectedModel, result);
    }
}