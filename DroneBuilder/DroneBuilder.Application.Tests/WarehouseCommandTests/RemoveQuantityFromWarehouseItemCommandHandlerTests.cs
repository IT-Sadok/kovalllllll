using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Mediator.Commands.WarehouseCommands;
using DroneBuilder.Application.Models.WarehouseModels;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.WarehouseEvents;
using FluentResults;
using NSubstitute;

namespace DroneBuilder.Application.Tests.WarehouseCommandTests;

public class RemoveQuantityFromWarehouseItemCommandHandlerTests
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IOutboxEventService _outboxService;
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

        var queuesConfig = new MessageQueuesConfiguration
        {
            WarehouseQueue = new QueueConfiguration { Name = WarehouseQueueName }
        };

        _handler = new RemoveQuantityFromWarehouseItemCommandHandler(
            _warehouseRepository,
            _outboxService,
            queuesConfig);
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

        // Act
        Result<WarehouseItemModel> result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(InitialQuantity - QuantityToRemove, result.Value.Quantity);
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
        Result<WarehouseItemModel> result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<NotFoundError>());

        Assert.Equal("Warehouse not found.", result.Errors[0].Message);

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
        Result<WarehouseItemModel> result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<NotFoundError>());

        Assert.Equal($"Warehouse item with id {WarehouseItemId} not found.", result.Errors[0].Message);

        await _outboxService.DidNotReceive().StoreEventAsync(
            Arg.Is<RemovedQuantityFromWarehouseItemEvent>(e => e.WarehouseItemId == WarehouseItemId),
            Arg.Is<string>(q => q == WarehouseQueueName),
            Arg.Any<CancellationToken>());

        await _warehouseRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

