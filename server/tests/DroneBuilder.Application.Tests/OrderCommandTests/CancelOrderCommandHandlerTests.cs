using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Mediator.Commands.OrderCommands;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using NSubstitute;

namespace DroneBuilder.Application.Tests.OrderCommandTests;

public class CancelOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly CancelOrderCommandHandler _handler;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid OrderId = Guid.NewGuid();
    private static readonly Guid ProductId = Guid.NewGuid();

    public CancelOrderCommandHandlerTests()
    {
        // Arrange
        _orderRepository = Substitute.For<IOrderRepository>();
        _warehouseRepository = Substitute.For<IWarehouseRepository>();
        _paymentGateway = Substitute.For<IPaymentGateway>();
        IUserContext userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(UserId);

        _handler = new CancelOrderCommandHandler(_orderRepository, _warehouseRepository, _paymentGateway, userContext);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenOwnNewOrder_ShouldCancelAndRestock()
    {
        // Arrange
        Order order = CreateOrder(UserId, Status.New);
        var warehouseItem = new WarehouseItem { ProductId = ProductId, Quantity = 10 };

        _orderRepository.GetOrderByIdAsync(OrderId, Arg.Any<CancellationToken>()).Returns(order);
        _warehouseRepository.GetWarehouseItemByProductIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(warehouseItem);

        // Act
        Result result = await _handler.ExecuteCommandAsync(new CancelOrderCommand(OrderId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(Status.Cancelled, order.Status);
        Assert.Equal(13, warehouseItem.Quantity);

        await _orderRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenOrderBelongsToAnotherUser_ShouldReturnNotFound()
    {
        // Arrange
        Order order = CreateOrder(Guid.NewGuid(), Status.New);
        _orderRepository.GetOrderByIdAsync(OrderId, Arg.Any<CancellationToken>()).Returns(order);

        // Act
        Result result = await _handler.ExecuteCommandAsync(new CancelOrderCommand(OrderId), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.True(result.HasError<NotFoundError>());
        Assert.Equal(Status.New, order.Status);

        await _orderRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(Status.Paid)]
    [InlineData(Status.Sent)]
    [InlineData(Status.Completed)]
    [InlineData(Status.Cancelled)]
    public async Task ExecuteCommandAsync_WhenOrderIsNotNew_ShouldReturnBadRequest(Status status)
    {
        // Arrange
        Order order = CreateOrder(UserId, status);
        _orderRepository.GetOrderByIdAsync(OrderId, Arg.Any<CancellationToken>()).Returns(order);

        // Act
        Result result = await _handler.ExecuteCommandAsync(new CancelOrderCommand(OrderId), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.True(result.HasError<BadRequestError>());
        Assert.Equal(status, order.Status);

        await _warehouseRepository.DidNotReceive()
            .GetWarehouseItemByProductIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenCheckoutIsOpen_ShouldExpireItAndCancel()
    {
        // Arrange
        Order order = CreateOrder(UserId, Status.New);
        order.PaymentSessionId = "cs_open";

        _orderRepository.GetOrderByIdAsync(OrderId, Arg.Any<CancellationToken>()).Returns(order);
        _paymentGateway.GetCheckoutSessionAsync("cs_open", Arg.Any<CancellationToken>())
            .Returns(new PaymentSession("cs_open", "https://checkout", PaymentSessionStatus.Open, 100));
        _warehouseRepository.GetWarehouseItemByProductIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(new WarehouseItem { ProductId = ProductId });

        // Act
        Result result = await _handler.ExecuteCommandAsync(new CancelOrderCommand(OrderId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(Status.Cancelled, order.Status);

        await _paymentGateway.Received(1).ExpireCheckoutSessionAsync("cs_open", Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(PaymentSessionStatus.Paid)]
    [InlineData(PaymentSessionStatus.Processing)]
    public async Task ExecuteCommandAsync_WhenCheckoutIsAlreadyPaid_ShouldRefuseToCancel(PaymentSessionStatus sessionStatus)
    {
        // Arrange
        Order order = CreateOrder(UserId, Status.New);
        order.PaymentSessionId = "cs_paid";

        _orderRepository.GetOrderByIdAsync(OrderId, Arg.Any<CancellationToken>()).Returns(order);
        _paymentGateway.GetCheckoutSessionAsync("cs_paid", Arg.Any<CancellationToken>())
            .Returns(new PaymentSession("cs_paid", null, sessionStatus, 100));

        // Act
        Result result = await _handler.ExecuteCommandAsync(new CancelOrderCommand(OrderId), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.True(result.HasError<BadRequestError>());
        Assert.Equal(Status.New, order.Status);

        await _warehouseRepository.DidNotReceive()
            .GetWarehouseItemByProductIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _orderRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static Order CreateOrder(Guid userId, Status status) => new()
    {
        Id = OrderId,
        UserId = userId,
        Status = status,
        OrderItems = [new OrderItem { ProductId = ProductId, Quantity = 3 }]
    };
}
