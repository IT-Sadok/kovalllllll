using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Mediator.Commands.WarehouseCommands;
using DroneBuilder.Application.Models.WarehouseModels;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.WarehouseEvents;
using FluentResults;
using MapsterMapper;
using NSubstitute;

namespace DroneBuilder.Application.Tests.WarehouseCommandTests;

public class AddQuantityToWarehouseItemCommandHandlerTests
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IOutboxEventService _outboxService;
    private readonly IMapper _mapper;
    private readonly AddQuantityToWarehouseItemCommandHandler _handler;

    private const string WarehouseQueueName = "warehouse-queue";
    private static readonly Guid WarehouseId = Guid.NewGuid();
    private static readonly Guid WarehouseItemId = Guid.NewGuid();
    private const int InitialQuantity = 50;
    private const int QuantityToAdd = 30;

    public AddQuantityToWarehouseItemCommandHandlerTests()
    {
        // Arrange - створення substitutes
        _warehouseRepository = Substitute.For<IWarehouseRepository>();
        _outboxService = Substitute.For<IOutboxEventService>();
        _mapper = Substitute.For<IMapper>();

        var queuesConfig = new MessageQueuesConfiguration
        {
            WarehouseQueue = new QueueConfiguration { Name = WarehouseQueueName }
        };

        _handler = new AddQuantityToWarehouseItemCommandHandler(
            _warehouseRepository,
            _outboxService,
            queuesConfig,
            _mapper);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenValidQuantity_ShouldAddQuantitySuccessfully()
    {
        // Arrange
        var addQuantityModel = new AddQuantityModel
        {
            QuantityToAdd = QuantityToAdd
        };
        var command = new AddQuantityToWarehouseItemCommand(WarehouseItemId, addQuantityModel);

        var warehouse = new Warehouse { Id = WarehouseId };

        var warehouseItem = new WarehouseItem
        {
            Id = WarehouseItemId,
            Quantity = InitialQuantity
        };

        var expectedModel = new WarehouseItemModel
        {
            Id = WarehouseItemId,
            Quantity = InitialQuantity + QuantityToAdd
        };

        _warehouseRepository.GetWarehouseAsync(Arg.Any<CancellationToken>())
            .Returns(warehouse);

        _warehouseRepository.GetWarehouseItemByIdAsync(
                Arg.Is<Guid>(id => id == WarehouseItemId),
                Arg.Any<CancellationToken>())
            .Returns(warehouseItem);

        _mapper.Map<WarehouseItemModel>(Arg.Is<WarehouseItem>(wi =>
                wi.Id == WarehouseItemId &&
                wi.Quantity == InitialQuantity + QuantityToAdd))
            .Returns(expectedModel);

        // Act
        Result<WarehouseItemModel> result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(InitialQuantity + QuantityToAdd, result.Value.Quantity);
        Assert.Equal(InitialQuantity + QuantityToAdd, warehouseItem.Quantity);

        await _outboxService.Received(1).StoreEventAsync(
            Arg.Is<AddedQuantityToWarehouseItemEvent>(e =>
                e.WarehouseItemId == WarehouseItemId &&
                e.QuantityAdded == QuantityToAdd),
            Arg.Is<string>(q => q == WarehouseQueueName),
            Arg.Any<CancellationToken>());

        await _warehouseRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenQuantityIsZero_ShouldThrowBadRequestException()
    {
        // Arrange
        var addQuantityModel = new AddQuantityModel
        {
            QuantityToAdd = 0
        };
        var command = new AddQuantityToWarehouseItemCommand(WarehouseItemId, addQuantityModel);

        // Act & Assert
        Result<WarehouseItemModel> result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<BadRequestError>());

        Assert.Equal("Quantity to add must be greater than 0.", result.Errors[0].Message);

        await _warehouseRepository.DidNotReceive().GetWarehouseAsync(Arg.Any<CancellationToken>());

        await _warehouseRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenQuantityIsNegative_ShouldThrowBadRequestException()
    {
        // Arrange
        var addQuantityModel = new AddQuantityModel
        {
            QuantityToAdd = -10
        };
        var command = new AddQuantityToWarehouseItemCommand(WarehouseItemId, addQuantityModel);

        // Act & Assert
        Result<WarehouseItemModel> result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<BadRequestError>());

        Assert.Equal("Quantity to add must be greater than 0.", result.Errors[0].Message);

        await _warehouseRepository.DidNotReceive().GetWarehouseAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenWarehouseNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var addQuantityModel = new AddQuantityModel
        {
            QuantityToAdd = QuantityToAdd
        };
        var command = new AddQuantityToWarehouseItemCommand(WarehouseItemId, addQuantityModel);

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
        var addQuantityModel = new AddQuantityModel
        {
            QuantityToAdd = QuantityToAdd
        };
        var command = new AddQuantityToWarehouseItemCommand(WarehouseItemId, addQuantityModel);

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
            Arg.Is<AddedQuantityToWarehouseItemEvent>(e => e.WarehouseItemId == WarehouseItemId),
            Arg.Is<string>(q => q == WarehouseQueueName),
            Arg.Any<CancellationToken>());

        await _warehouseRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenSuccessful_ShouldGenerateCorrectEvent()
    {
        // Arrange
        var addQuantityModel = new AddQuantityModel
        {
            QuantityToAdd = 25
        };
        var command = new AddQuantityToWarehouseItemCommand(WarehouseItemId, addQuantityModel);

        var warehouse = new Warehouse { Id = WarehouseId };

        var warehouseItem = new WarehouseItem
        {
            Id = WarehouseItemId,
            Quantity = 100
        };

        _warehouseRepository.GetWarehouseAsync(Arg.Any<CancellationToken>())
            .Returns(warehouse);

        _warehouseRepository.GetWarehouseItemByIdAsync(
                Arg.Is<Guid>(id => id == WarehouseItemId),
                Arg.Any<CancellationToken>())
            .Returns(warehouseItem);

        _mapper.Map<WarehouseItemModel>(Arg.Is<WarehouseItem>(wi => wi.Id == WarehouseItemId))
            .Returns(new WarehouseItemModel());

        Guid capturedItemId = Guid.Empty;
        int capturedQuantity = 0;

        await _outboxService.StoreEventAsync(
            Arg.Do<AddedQuantityToWarehouseItemEvent>(e =>
            {
                capturedItemId = e.WarehouseItemId;
                capturedQuantity = e.QuantityAdded;
            }),
            Arg.Is<string>(q => q == WarehouseQueueName),
            Arg.Any<CancellationToken>());

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(WarehouseItemId, capturedItemId);
        Assert.Equal(25, capturedQuantity);
    }
}
