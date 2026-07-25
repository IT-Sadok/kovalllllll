using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Features.Orders.PayForOrder;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using NSubstitute;

namespace DroneBuilder.Application.Tests.OrderCommandTests;

public class PayForOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly PayForOrderCommandHandler _handler;
    private readonly IUserContext _userContext;

    private static readonly Guid OrderId = Guid.NewGuid();

    public PayForOrderCommandHandlerTests()
    {
        // Arrange
        _orderRepository = Substitute.For<IOrderRepository>();
        _userContext = Substitute.For<IUserContext>();
        _userContext.UserId.Returns(Guid.Empty);

        _handler = new PayForOrderCommandHandler(_orderRepository, _userContext);
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
            .Returns((Order?)null);

        // Act & Assert
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<NotFoundError>());

        Assert.Equal($"Order with id {OrderId} not found.", result.Errors[0].Message);

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
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<BadRequestError>());

        Assert.Equal("Order is not in new status.", result.Errors[0].Message);

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
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<BadRequestError>());

        Assert.Equal("Order is already paid.", result.Errors[0].Message);

        await _orderRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task ExecuteCommandAsync_WhenOrderBelongsToAnotherUser_ShouldReturnForbidden()
    {
        Guid currentUserId = Guid.NewGuid();
        _userContext.UserId.Returns(currentUserId);
        var order = new Order
        {
            Id = OrderId,
            UserId = Guid.NewGuid(),
            Status = Status.New
        };
        _orderRepository.GetOrderByIdAsync(OrderId, Arg.Any<CancellationToken>()).Returns(order);

        Result result = await _handler.ExecuteCommandAsync(
            new PayForOrderCommand(OrderId),
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<ForbiddenError>());
        Assert.Equal(Status.New, order.Status);
        await _orderRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
