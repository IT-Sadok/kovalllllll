using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Commands.CartCommands;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.CartEvents;
using NSubstitute;

namespace DroneBuilder.Application.Tests.CartCommandTests;

public class ClearCartCommandHandlerTests
{
    private readonly ICartRepository _cartRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IOutboxEventService _outboxService;
    private readonly ClearCartCommandHandler _handler;

    private const string CartQueueName = "cart-queue";
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid CartId = Guid.NewGuid();
    private static readonly Guid ProductId1 = Guid.NewGuid();
    private static readonly Guid ProductId2 = Guid.NewGuid();
    private const int ItemCount = 3;

    public ClearCartCommandHandlerTests()
    {
        // Arrange
        _cartRepository = Substitute.For<ICartRepository>();
        _warehouseRepository = Substitute.For<IWarehouseRepository>();
        _outboxService = Substitute.For<IOutboxEventService>();
        var userContext = Substitute.For<IUserContext>();

        var queuesConfig = new MessageQueuesConfiguration
        {
            CartQueue = new QueueConfiguration { Name = CartQueueName }
        };

        userContext.UserId.Returns(UserId);

        _handler = new ClearCartCommandHandler(
            _cartRepository,
            _warehouseRepository,
            _outboxService,
            queuesConfig,
            userContext);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenCartExists_ShouldClearCartAndReturnItemsToWarehouse()
    {
        // Arrange
        var command = new ClearCartCommand();

        var cartItem1 = new CartItem
        {
            ProductId = ProductId1,
            ProductName = "Product 1",
            Quantity = 5
        };

        var cartItem2 = new CartItem
        {
            ProductId = ProductId2,
            ProductName = "Product 2",
            Quantity = 3
        };

        var cart = new Cart
        {
            Id = CartId,
            UserId = UserId,
            CartItems = new List<CartItem> { cartItem1, cartItem2 }
        };

        var warehouseItem1 = new WarehouseItem
        {
            ProductId = ProductId1,
            Quantity = 100
        };

        var warehouseItem2 = new WarehouseItem
        {
            ProductId = ProductId2,
            Quantity = 50
        };

        _cartRepository.GetCartByUserIdAsync(UserId, Arg.Any<CancellationToken>())
            .Returns(cart);

        _warehouseRepository.GetWarehouseItemByProductIdAsync(ProductId1, Arg.Any<CancellationToken>())
            .Returns(warehouseItem1);

        _warehouseRepository.GetWarehouseItemByProductIdAsync(ProductId2, Arg.Any<CancellationToken>())
            .Returns(warehouseItem2);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(105, warehouseItem1.Quantity);
        Assert.Equal(53, warehouseItem2.Quantity);

        await _cartRepository.Received(1).ClearCartAsync(CartId, Arg.Any<CancellationToken>());

        await _outboxService.Received(1).StoreEventAsync(
            Arg.Is<ClearedCartEvent>(e => e.UserId == UserId),
            CartQueueName,
            Arg.Any<CancellationToken>());

        await _cartRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenCartNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new ClearCartCommand();

        _cartRepository.GetCartByUserIdAsync(UserId, Arg.Any<CancellationToken>())
            .Returns((Cart)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal($"Cart for user ID {UserId} not found.", exception.Message);

        await _warehouseRepository.DidNotReceive().GetWarehouseItemByProductIdAsync(
            Arg.Is<Guid>(id => id == ProductId1),
            Arg.Any<CancellationToken>());

        await _cartRepository.DidNotReceive().ClearCartAsync(
            Arg.Is<Guid>(id => id == CartId),
            Arg.Any<CancellationToken>());

        await _outboxService.DidNotReceive().StoreEventAsync(
            Arg.Is<ClearedCartEvent>(e => e.UserId == UserId),
            Arg.Is<string>(q => q == CartQueueName),
            Arg.Any<CancellationToken>());

        await _cartRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenWarehouseItemNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new ClearCartCommand();

        var cartItem = new CartItem
        {
            ProductId = ProductId1,
            ProductName = "Product 1",
            Quantity = 5
        };

        var cart = new Cart
        {
            Id = CartId,
            UserId = UserId,
            CartItems = new List<CartItem> { cartItem }
        };

        _cartRepository.GetCartByUserIdAsync(UserId, Arg.Any<CancellationToken>())
            .Returns(cart);

        _warehouseRepository.GetWarehouseItemByProductIdAsync(ProductId1, Arg.Any<CancellationToken>())
            .Returns((WarehouseItem)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Contains($"Warehouse item for product {ProductId1} not found while clearing cart.", exception.Message);

        await _cartRepository.DidNotReceive().ClearCartAsync(
            Arg.Is<Guid>(id => id == CartId),
            Arg.Any<CancellationToken>());

        await _outboxService.DidNotReceive().StoreEventAsync(
            Arg.Is<ClearedCartEvent>(e => e.UserId == UserId),
            Arg.Is<string>(q => q == CartQueueName),
            Arg.Any<CancellationToken>());

        await _cartRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenCartIsEmpty_ShouldClearCartWithoutWarehouseChanges()
    {
        // Arrange
        var command = new ClearCartCommand();

        var cart = new Cart
        {
            Id = CartId,
            UserId = UserId,
            CartItems = new List<CartItem>()
        };

        _cartRepository.GetCartByUserIdAsync(UserId, Arg.Any<CancellationToken>())
            .Returns(cart);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        await _warehouseRepository.DidNotReceive().GetWarehouseItemByProductIdAsync(
            Arg.Is<Guid>(id => cart.CartItems.Any(ci => ci.ProductId == id)),
            Arg.Any<CancellationToken>());

        await _cartRepository.Received(1).ClearCartAsync(CartId, Arg.Any<CancellationToken>());

        await _outboxService.Received(1).StoreEventAsync(
            Arg.Is<ClearedCartEvent>(e => e.UserId == UserId),
            CartQueueName,
            Arg.Any<CancellationToken>());

        await _cartRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenMultipleItems_ShouldUpdateAllWarehouseItems()
    {
        // Arrange
        var command = new ClearCartCommand();

        var cartItems = new List<CartItem>
        {
            new() { ProductId = Guid.NewGuid(), Quantity = 10 },
            new() { ProductId = Guid.NewGuid(), Quantity = 20 },
            new() { ProductId = Guid.NewGuid(), Quantity = 30 }
        };

        var cart = new Cart
        {
            Id = CartId,
            UserId = UserId,
            CartItems = cartItems
        };

        _cartRepository.GetCartByUserIdAsync(UserId, Arg.Any<CancellationToken>())
            .Returns(cart);

        foreach (var cartItem in cartItems)
        {
            var warehouseItem = new WarehouseItem
            {
                ProductId = cartItem.ProductId,
                Quantity = 100
            };
            _warehouseRepository.GetWarehouseItemByProductIdAsync(cartItem.ProductId, Arg.Any<CancellationToken>())
                .Returns(warehouseItem);
        }

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        await _warehouseRepository.Received(ItemCount).GetWarehouseItemByProductIdAsync(
            Arg.Is<Guid>(id => cartItems.Any(ci => ci.ProductId == id)),
            Arg.Any<CancellationToken>());

        await _cartRepository.Received(1).ClearCartAsync(CartId, Arg.Any<CancellationToken>());
        await _cartRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenSuccessful_ShouldStoreCorrectEvent()
    {
        // Arrange
        var command = new ClearCartCommand();

        var cart = new Cart
        {
            Id = CartId,
            UserId = UserId,
            CartItems = new List<CartItem>()
        };

        _cartRepository.GetCartByUserIdAsync(UserId, Arg.Any<CancellationToken>())
            .Returns(cart);

        ClearedCartEvent capturedEvent = null;
        await _outboxService.StoreEventAsync(
            Arg.Do<ClearedCartEvent>(e => capturedEvent = e),
            Arg.Is<string>(q => q == CartQueueName),
            Arg.Any<CancellationToken>());

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedEvent);
        Assert.Equal(UserId, capturedEvent.UserId);

        await _outboxService.Received(1).StoreEventAsync(
            Arg.Is<ClearedCartEvent>(e => e.UserId == UserId),
            CartQueueName,
            Arg.Any<CancellationToken>());
    }
}