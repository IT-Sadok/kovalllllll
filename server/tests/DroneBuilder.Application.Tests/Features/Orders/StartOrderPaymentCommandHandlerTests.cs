using DroneBuilder.Application.Common.Abstractions;
using DroneBuilder.Application.Common.Contexts;
using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Features.Orders.StartOrderPayment;
using DroneBuilder.Domain.Entities;
using FluentResults;
using NSubstitute;
namespace DroneBuilder.Application.Tests.Features.Orders;

public class StartOrderPaymentCommandHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly StartOrderPaymentCommandHandler _handler;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid OrderId = Guid.NewGuid();
    private const decimal OrderTotal = 123.45m;
    private const long OrderTotalMinor = 12345;

    public StartOrderPaymentCommandHandlerTests()
    {
        // Arrange
        _orderRepository = Substitute.For<IOrderRepository>();
        _paymentGateway = Substitute.For<IPaymentGateway>();
        IUserContext userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(UserId);

        _handler = new StartOrderPaymentCommandHandler(_orderRepository, _paymentGateway, userContext);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenNoSessionYet_ShouldCreateOneAndReturnItsUrl()
    {
        // Arrange
        Order order = CreateOrder(UserId, Status.New);
        _orderRepository.GetOrderByIdAsync(OrderId, Arg.Any<CancellationToken>()).Returns(order);
        _paymentGateway.CreateCheckoutSessionAsync(order, Arg.Any<CancellationToken>())
            .Returns(new PaymentSession("cs_new", "https://checkout/new", PaymentSessionStatus.Open, OrderTotalMinor));

        // Act
        Result<PaymentSessionModel> result =
            await _handler.ExecuteCommandAsync(new StartOrderPaymentCommand(OrderId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("https://checkout/new", result.Value.Url);
        Assert.False(result.Value.IsPaid);
        Assert.Equal("cs_new", order.PaymentSessionId);
        Assert.Equal(Status.New, order.Status);

        await _orderRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenSessionIsStillOpen_ShouldReuseIt()
    {
        // Arrange
        Order order = CreateOrder(UserId, Status.New);
        order.PaymentSessionId = "cs_open";
        _orderRepository.GetOrderByIdAsync(OrderId, Arg.Any<CancellationToken>()).Returns(order);
        _paymentGateway.GetCheckoutSessionAsync("cs_open", Arg.Any<CancellationToken>())
            .Returns(new PaymentSession("cs_open", "https://checkout/open", PaymentSessionStatus.Open, OrderTotalMinor));

        // Act
        Result<PaymentSessionModel> result =
            await _handler.ExecuteCommandAsync(new StartOrderPaymentCommand(OrderId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("https://checkout/open", result.Value.Url);

        await _paymentGateway.DidNotReceive().CreateCheckoutSessionAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenPreviousSessionExpired_ShouldCreateANewOne()
    {
        // Arrange
        Order order = CreateOrder(UserId, Status.New);
        order.PaymentSessionId = "cs_old";
        _orderRepository.GetOrderByIdAsync(OrderId, Arg.Any<CancellationToken>()).Returns(order);
        _paymentGateway.GetCheckoutSessionAsync("cs_old", Arg.Any<CancellationToken>())
            .Returns(new PaymentSession("cs_old", null, PaymentSessionStatus.Expired, OrderTotalMinor));
        _paymentGateway.CreateCheckoutSessionAsync(order, Arg.Any<CancellationToken>())
            .Returns(new PaymentSession("cs_new", "https://checkout/new", PaymentSessionStatus.Open, OrderTotalMinor));

        // Act
        Result<PaymentSessionModel> result =
            await _handler.ExecuteCommandAsync(new StartOrderPaymentCommand(OrderId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("cs_new", order.PaymentSessionId);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenPreviousSessionWasPaid_ShouldMarkOrderPaidWithoutNewSession()
    {
        // Arrange
        Order order = CreateOrder(UserId, Status.New);
        order.PaymentSessionId = "cs_paid";
        _orderRepository.GetOrderByIdAsync(OrderId, Arg.Any<CancellationToken>()).Returns(order);
        _paymentGateway.GetCheckoutSessionAsync("cs_paid", Arg.Any<CancellationToken>())
            .Returns(new PaymentSession("cs_paid", null, PaymentSessionStatus.Paid, OrderTotalMinor));

        // Act
        Result<PaymentSessionModel> result =
            await _handler.ExecuteCommandAsync(new StartOrderPaymentCommand(OrderId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsPaid);
        Assert.Null(result.Value.Url);
        Assert.Equal(Status.Paid, order.Status);

        await _paymentGateway.DidNotReceive().CreateCheckoutSessionAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenPaymentIsProcessing_ShouldReturnBadRequest()
    {
        // Arrange
        Order order = CreateOrder(UserId, Status.New);
        order.PaymentSessionId = "cs_processing";
        _orderRepository.GetOrderByIdAsync(OrderId, Arg.Any<CancellationToken>()).Returns(order);
        _paymentGateway.GetCheckoutSessionAsync("cs_processing", Arg.Any<CancellationToken>())
            .Returns(new PaymentSession("cs_processing", null, PaymentSessionStatus.Processing, OrderTotalMinor));

        // Act
        Result<PaymentSessionModel> result =
            await _handler.ExecuteCommandAsync(new StartOrderPaymentCommand(OrderId), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.True(result.HasError<BadRequestError>());

        await _paymentGateway.DidNotReceive().CreateCheckoutSessionAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenOrderBelongsToAnotherUser_ShouldReturnNotFound()
    {
        // Arrange
        _orderRepository.GetOrderByIdAsync(OrderId, Arg.Any<CancellationToken>())
            .Returns(CreateOrder(Guid.NewGuid(), Status.New));

        // Act
        Result<PaymentSessionModel> result =
            await _handler.ExecuteCommandAsync(new StartOrderPaymentCommand(OrderId), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.True(result.HasError<NotFoundError>());

        await _paymentGateway.DidNotReceive().CreateCheckoutSessionAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(Status.Paid)]
    [InlineData(Status.Sent)]
    [InlineData(Status.Completed)]
    [InlineData(Status.Cancelled)]
    public async Task ExecuteCommandAsync_WhenOrderIsNotNew_ShouldReturnBadRequest(Status status)
    {
        // Arrange
        _orderRepository.GetOrderByIdAsync(OrderId, Arg.Any<CancellationToken>())
            .Returns(CreateOrder(UserId, status));

        // Act
        Result<PaymentSessionModel> result =
            await _handler.ExecuteCommandAsync(new StartOrderPaymentCommand(OrderId), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.True(result.HasError<BadRequestError>());
    }

    private static Order CreateOrder(Guid userId, Status status) => new()
    {
        Id = OrderId,
        UserId = userId,
        Status = status,
        TotalPrice = OrderTotal
    };
}
