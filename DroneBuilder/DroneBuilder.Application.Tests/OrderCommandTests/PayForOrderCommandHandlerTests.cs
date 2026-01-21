using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Commands.OrderCommands;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using NSubstitute;

namespace DroneBuilder.Application.Tests.OrderCommandTests;

public class PayForOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly PayForOrderCommandHandler _handler;

    private static readonly Guid OrderId = Guid.NewGuid();

    public PayForOrderCommandHandlerTests()
    {
        // Arrange
        _orderRepository = Substitute.For<IOrderRepository>();

        _handler = new PayForOrderCommandHandler(_orderRepository);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenOrderIsNew_ShouldMarkAsPaid()
    {
        // Arrange
        var command = new PayForOrderCommand(OrderId);

        var order = new Order
        {
            Id = OrderId,
            Status = Status.New
        };

        _orderRepository.GetOrderByIdAsync(
                Arg.Is<Guid>(id => id == OrderId),
                Arg.Any<CancellationToken>())
            .Returns(order);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(Status.Paid, order.Status);

        await _orderRepository.Received(1).GetOrderByIdAsync(
            Arg.Is<Guid>(id => id == OrderId),
            Arg.Any<CancellationToken>());

        await _orderRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenOrderNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new PayForOrderCommand(OrderId);

        _orderRepository.GetOrderByIdAsync(
                Arg.Is<Guid>(id => id == OrderId),
                Arg.Any<CancellationToken>())
            .Returns((Order)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal($"Order with id {OrderId} not found.", exception.Message);

        await _orderRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenOrderNotInNewStatus_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new PayForOrderCommand(OrderId);

        var order = new Order
        {
            Id = OrderId,
            Status = Status.Sent
        };

        _orderRepository.GetOrderByIdAsync(
                Arg.Is<Guid>(id => id == OrderId),
                Arg.Any<CancellationToken>())
            .Returns(order);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal("Order is not in new status.", exception.Message);

        Assert.NotEqual(Status.Paid, order.Status);

        await _orderRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenOrderAlreadyPaid_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new PayForOrderCommand(OrderId);

        var order = new Order
        {
            Id = OrderId,
            Status = Status.Paid
        };

        _orderRepository.GetOrderByIdAsync(
                Arg.Is<Guid>(id => id == OrderId),
                Arg.Any<CancellationToken>())
            .Returns(order);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal("Order is already paid.", exception.Message);

        await _orderRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}