using DroneBuilder.Application.Common.Abstractions;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Common.ResultErrors;
using DroneBuilder.Application.Features.Orders.ConfirmOrderPayment;
using DroneBuilder.Domain.Entities;
using FluentResults;
using NSubstitute;
namespace DroneBuilder.Application.Tests.Features.Orders;

public class ConfirmOrderPaymentCommandHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly ConfirmOrderPaymentCommandHandler _handler;

    private static readonly Guid OrderId = Guid.NewGuid();
    private const string SessionId = "cs_test";
    private const decimal OrderTotal = 99.99m;
    private const long OrderTotalMinor = 9999;

    public ConfirmOrderPaymentCommandHandlerTests()
    {
        // Arrange
        _orderRepository = Substitute.For<IOrderRepository>();
        _handler = new ConfirmOrderPaymentCommandHandler(_orderRepository);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenPaymentMatchesOrder_ShouldMarkItPaid()
    {
        // Arrange
        Order order = CreateOrder(Status.New);
        _orderRepository.GetOrderByIdAsync(OrderId, Arg.Any<CancellationToken>()).Returns(order);

        // Act
        Result result = await _handler.ExecuteCommandAsync(Command(), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(Status.Paid, order.Status);

        await _orderRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenOrderIsAlreadyPaid_ShouldBeIdempotent()
    {
        // Arrange
        Order order = CreateOrder(Status.Paid);
        _orderRepository.GetOrderByIdAsync(OrderId, Arg.Any<CancellationToken>()).Returns(order);

        // Act
        Result result = await _handler.ExecuteCommandAsync(Command(), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(Status.Paid, order.Status);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenSessionIsNotTheOrdersCurrentOne_ShouldReject()
    {
        // Arrange
        Order order = CreateOrder(Status.New);
        order.PaymentSessionId = "cs_other";
        _orderRepository.GetOrderByIdAsync(OrderId, Arg.Any<CancellationToken>()).Returns(order);

        // Act
        Result result = await _handler.ExecuteCommandAsync(Command(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal(Status.New, order.Status);

        await _orderRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenAmountDiffers_ShouldReturnConflict()
    {
        // Arrange
        Order order = CreateOrder(Status.New);
        _orderRepository.GetOrderByIdAsync(OrderId, Arg.Any<CancellationToken>()).Returns(order);

        // Act
        Result result = await _handler.ExecuteCommandAsync(Command(amount: OrderTotalMinor - 1), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.True(result.HasError<ConflictError>());
        Assert.Equal(Status.New, order.Status);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenOrderWasCancelled_ShouldReturnConflictForRefund()
    {
        // Arrange
        Order order = CreateOrder(Status.Cancelled);
        _orderRepository.GetOrderByIdAsync(OrderId, Arg.Any<CancellationToken>()).Returns(order);

        // Act
        Result result = await _handler.ExecuteCommandAsync(Command(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.True(result.HasError<ConflictError>());
        Assert.Equal(Status.Cancelled, order.Status);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenSessionIsNotPaid_ShouldIgnoreIt()
    {
        // Act
        Result result = await _handler.ExecuteCommandAsync(Command(isPaid: false), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        await _orderRepository.DidNotReceive().GetOrderByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    private static ConfirmOrderPaymentCommand Command(bool isPaid = true, long amount = OrderTotalMinor)
        => new(new PaymentNotification(SessionId, OrderId, isPaid, amount));

    private static Order CreateOrder(Status status) => new()
    {
        Id = OrderId,
        Status = status,
        TotalPrice = OrderTotal,
        PaymentSessionId = SessionId
    };
}
