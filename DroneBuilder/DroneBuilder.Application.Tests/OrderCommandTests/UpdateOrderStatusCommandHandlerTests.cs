using DroneBuilder.Application.Mediator.Commands.OrderCommands;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using NSubstitute;

namespace DroneBuilder.Application.Tests.OrderCommandTests;

public class UpdateOrderStatusCommandHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly UpdateOrderStatusCommandHandler _handler;

    private static readonly Guid OrderId = Guid.NewGuid();
    private static readonly Guid ProductId = Guid.NewGuid();

    public UpdateOrderStatusCommandHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _warehouseRepository = Substitute.For<IWarehouseRepository>();
        _handler = new UpdateOrderStatusCommandHandler(_orderRepository, _warehouseRepository);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenTransitionIsAllowed_ShouldUpdateStatus()
    {
        // Arrange
        var command = new UpdateOrderStatusCommand(OrderId, Status.Sent);
        var order = new Order { Id = OrderId, Status = Status.Paid };

        _orderRepository.GetOrderByIdAsync(OrderId, Arg.Any<CancellationToken>()).Returns(order);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(Status.Sent, order.Status);
        await _orderRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenOrderNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UpdateOrderStatusCommand(OrderId, Status.Sent);
        _orderRepository.GetOrderByIdAsync(OrderId, Arg.Any<CancellationToken>()).Returns((Order)null!);

        // Act & Assert
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenInvalidStatus_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new UpdateOrderStatusCommand(OrderId, (Status)999);
        var order = new Order { Id = OrderId, Status = Status.New };

        _orderRepository.GetOrderByIdAsync(OrderId, Arg.Any<CancellationToken>()).Returns(order);

        // Act & Assert
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<BadRequestError>());
    }

    [Theory]
    [InlineData(Status.New, Status.Sent)]
    [InlineData(Status.New, Status.Completed)]
    [InlineData(Status.Sent, Status.Cancelled)]
    [InlineData(Status.Completed, Status.New)]
    [InlineData(Status.Cancelled, Status.Paid)]
    public async Task ExecuteCommandAsync_WhenTransitionIsNotAllowed_ShouldThrowBadRequestException(
        Status currentStatus,
        Status newStatus)
    {
        // Arrange
        var command = new UpdateOrderStatusCommand(OrderId, newStatus);
        var order = new Order { Id = OrderId, Status = currentStatus };

        _orderRepository.GetOrderByIdAsync(OrderId, Arg.Any<CancellationToken>()).Returns(order);

        // Act & Assert
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<BadRequestError>());

        Assert.Equal(currentStatus, order.Status);

        await _orderRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenOrderIsCancelled_ShouldReturnQuantityToWarehouse()
    {
        // Arrange
        var command = new UpdateOrderStatusCommand(OrderId, Status.Cancelled);

        var order = new Order
        {
            Id = OrderId,
            Status = Status.Paid,
            OrderItems = [new OrderItem { ProductId = ProductId, Quantity = 3 }]
        };

        var warehouseItem = new WarehouseItem { ProductId = ProductId, Quantity = 7 };

        _orderRepository.GetOrderByIdAsync(OrderId, Arg.Any<CancellationToken>()).Returns(order);

        _warehouseRepository.GetWarehouseItemByProductIdAsync(
                Arg.Is<Guid>(id => id == ProductId),
                Arg.Any<CancellationToken>())
            .Returns(warehouseItem);

        // Act
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(Status.Cancelled, order.Status);
        Assert.Equal(10, warehouseItem.Quantity);

        await _orderRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenWarehouseItemMissingOnCancel_ShouldRecreateItAndRestock()
    {
        // Arrange
        var command = new UpdateOrderStatusCommand(OrderId, Status.Cancelled);
        var warehouse = new Warehouse { Id = Guid.NewGuid() };

        var order = new Order
        {
            Id = OrderId,
            Status = Status.New,
            OrderItems = [new OrderItem { ProductId = ProductId, Quantity = 3 }]
        };

        _orderRepository.GetOrderByIdAsync(OrderId, Arg.Any<CancellationToken>()).Returns(order);
        _warehouseRepository.GetWarehouseAsync(Arg.Any<CancellationToken>()).Returns(warehouse);

        _warehouseRepository.GetWarehouseItemByProductIdAsync(
                Arg.Is<Guid>(id => id == ProductId),
                Arg.Any<CancellationToken>())
            .Returns((WarehouseItem)null!);

        // Act
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(Status.Cancelled, order.Status);

        await _warehouseRepository.Received(1).AddWarehouseItemAsync(
            Arg.Is<WarehouseItem>(wi =>
                wi.ProductId == ProductId && wi.WarehouseId == warehouse.Id && wi.Quantity == 3),
            Arg.Any<CancellationToken>());

        await _orderRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenWarehouseMissingOnCancel_ShouldReturnNotFound()
    {
        // Arrange
        var command = new UpdateOrderStatusCommand(OrderId, Status.Cancelled);

        var order = new Order
        {
            Id = OrderId,
            Status = Status.New,
            OrderItems = [new OrderItem { ProductId = ProductId, Quantity = 3 }]
        };

        _orderRepository.GetOrderByIdAsync(OrderId, Arg.Any<CancellationToken>()).Returns(order);
        _warehouseRepository.GetWarehouseAsync(Arg.Any<CancellationToken>()).Returns((Warehouse)null!);

        _warehouseRepository.GetWarehouseItemByProductIdAsync(
                Arg.Is<Guid>(id => id == ProductId),
                Arg.Any<CancellationToken>())
            .Returns((WarehouseItem)null!);

        // Act
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.True(result.HasError<NotFoundError>());
        Assert.Equal(Status.New, order.Status);

        await _orderRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
