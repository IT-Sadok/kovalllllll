using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Commands.WarehouseCommands;
using DroneBuilder.Application.Models.WarehouseModels;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.WarehouseEvents;
using MapsterMapper;
using NSubstitute;

namespace DroneBuilder.Application.Tests.WarehouseCommandTests;

public class RemoveQuantityFromWarehouseItemCommandHandlerTests
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IOutboxEventService _outboxService;
    private readonly IMapper _mapper;
    private readonly RemoveQuantityFromWarehouseItemCommandHandler _handler;

    private const string WarehouseQueueName = "warehouse-queue";
    private static readonly Guid WarehouseId = Guid.NewGuid();
    private static readonly Guid WarehouseItemId = Guid.NewGuid();
    private const int InitialQuantity = 100;
    private const int QuantityToRemove = 30;

    public RemoveQuantityFromWarehouseItemCommandHandlerTests()
    {
        // Arrange
        _warehouseRepository = Substitute.For<IWarehouseRepository>();
        _outboxService = Substitute.For<IOutboxEventService>();
        _mapper = Substitute.For<IMapper>();

        var queuesConfig = new MessageQueuesConfiguration
        {
            WarehouseQueue = new QueueConfiguration { Name = WarehouseQueueName }
        };

        _handler = new RemoveQuantityFromWarehouseItemCommandHandler(
            _warehouseRepository,
            _outboxService,
            queuesConfig,
            _mapper);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenValidQuantity_ShouldRemoveQuantitySuccessfully()
    {
        // Arrange
        var removeQuantityModel = new RemoveQuantityModel
        {
            QuantityToRemove = QuantityToRemove
        };
        var command = new RemoveQuantityFromWarehouseItemCommand(WarehouseItemId, removeQuantityModel);

        var warehouse = new Warehouse { Id = WarehouseId };

        var warehouseItem = new WarehouseItem
        {
            Id = WarehouseItemId,
            Quantity = InitialQuantity
        };

        var expectedModel = new WarehouseItemModel
        {
            Id = WarehouseItemId,
            Quantity = InitialQuantity - QuantityToRemove
        };

        _warehouseRepository.GetWarehouseAsync(Arg.Any<CancellationToken>())
            .Returns(warehouse);

        _warehouseRepository.GetWarehouseItemByIdAsync(
                Arg.Is<Guid>(id => id == WarehouseItemId),
                Arg.Any<CancellationToken>())
            .Returns(warehouseItem);

        _mapper.Map<WarehouseItemModel>(Arg.Is<WarehouseItem>(wi =>
                wi.Id == WarehouseItemId &&
                wi.Quantity == InitialQuantity - QuantityToRemove))
            .Returns(expectedModel);

        // Act
        var result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(InitialQuantity - QuantityToRemove, result.Quantity);
        Assert.Equal(InitialQuantity - QuantityToRemove, warehouseItem.Quantity);

        await _outboxService.Received(1).StoreEventAsync(
            Arg.Is<RemovedQuantityFromWarehouseItemEvent>(e =>
                e.WarehouseItemId == WarehouseItemId &&
                e.QuantityRemoved == QuantityToRemove),
            Arg.Is<string>(q => q == WarehouseQueueName),
            Arg.Any<CancellationToken>());

        await _warehouseRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenQuantityIsZero_ShouldThrowBadRequestException()
    {
        // Arrange
        var removeQuantityModel = new RemoveQuantityModel
        {
            QuantityToRemove = 0
        };
        var command = new RemoveQuantityFromWarehouseItemCommand(WarehouseItemId, removeQuantityModel);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal("Quantity to remove must be greater than 0.", exception.Message);

        await _warehouseRepository.DidNotReceive().GetWarehouseAsync(Arg.Any<CancellationToken>());

        await _warehouseRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenQuantityIsNegative_ShouldThrowBadRequestException()
    {
        // Arrange
        var removeQuantityModel = new RemoveQuantityModel
        {
            QuantityToRemove = -10
        };
        var command = new RemoveQuantityFromWarehouseItemCommand(WarehouseItemId, removeQuantityModel);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal("Quantity to remove must be greater than 0.", exception.Message);

        await _warehouseRepository.DidNotReceive().GetWarehouseAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenWarehouseNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var removeQuantityModel = new RemoveQuantityModel
        {
            QuantityToRemove = QuantityToRemove
        };
        var command = new RemoveQuantityFromWarehouseItemCommand(WarehouseItemId, removeQuantityModel);

        _warehouseRepository.GetWarehouseAsync(Arg.Any<CancellationToken>())
            .Returns((Warehouse)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal("Warehouse not found.", exception.Message);

        await _warehouseRepository.DidNotReceive().GetWarehouseItemByIdAsync(
            Arg.Is<Guid>(id => id == WarehouseItemId),
            Arg.Any<CancellationToken>());

        await _warehouseRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenWarehouseItemNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var removeQuantityModel = new RemoveQuantityModel
        {
            QuantityToRemove = QuantityToRemove
        };
        var command = new RemoveQuantityFromWarehouseItemCommand(WarehouseItemId, removeQuantityModel);

        var warehouse = new Warehouse { Id = WarehouseId };

        _warehouseRepository.GetWarehouseAsync(Arg.Any<CancellationToken>())
            .Returns(warehouse);

        _warehouseRepository.GetWarehouseItemByIdAsync(
                Arg.Is<Guid>(id => id == WarehouseItemId),
                Arg.Any<CancellationToken>())
            .Returns((WarehouseItem)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal($"Warehouse item with id {WarehouseItemId} not found.", exception.Message);

        await _outboxService.DidNotReceive().StoreEventAsync(
            Arg.Is<RemovedQuantityFromWarehouseItemEvent>(e => e.WarehouseItemId == WarehouseItemId),
            Arg.Is<string>(q => q == WarehouseQueueName),
            Arg.Any<CancellationToken>());

        await _warehouseRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}