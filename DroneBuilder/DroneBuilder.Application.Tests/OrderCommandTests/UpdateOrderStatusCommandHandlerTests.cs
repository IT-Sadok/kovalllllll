using DroneBuilder.Application.Features.Orders.UpdateOrderStatus;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using NSubstitute;

namespace DroneBuilder.Application.Tests.OrderCommandTests;

public class UpdateOrderStatusCommandHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly UpdateOrderStatusCommandHandler _handler;

    private static readonly Guid OrderId = Guid.NewGuid();

    public UpdateOrderStatusCommandHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _handler = new UpdateOrderStatusCommandHandler(_orderRepository);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenOrderExists_ShouldUpdateStatus()
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
        _orderRepository.GetOrderByIdAsync(OrderId, Arg.Any<CancellationToken>()).Returns((Order?)null);

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
        var order = new Order { Id = OrderId, Status = Status.Paid };

        _orderRepository.GetOrderByIdAsync(OrderId, Arg.Any<CancellationToken>()).Returns(order);

        // Act & Assert
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<BadRequestError>());
    }
}
