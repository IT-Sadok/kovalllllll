using DroneBuilder.Application.Common.Abstractions;
using DroneBuilder.Application.Common.Contexts;
using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Options;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Features.Carts.UpdateCartItemQuantity;
using DroneBuilder.Domain.Entities;
using FluentResults;
using NSubstitute;
namespace DroneBuilder.Application.Tests.Features.Carts;

public class UpdateCartItemQuantityCommandHandlerTests
{
    private readonly ICartRepository _cartRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IOutboxEventService _outboxService;
    private readonly IUserContext _userContext;
    private readonly UpdateCartItemQuantityCommandHandler _handler;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ProductId = Guid.NewGuid();
    private static readonly Guid CartItemId = Guid.NewGuid();

    public UpdateCartItemQuantityCommandHandlerTests()
    {
        _cartRepository = Substitute.For<ICartRepository>();
        _warehouseRepository = Substitute.For<IWarehouseRepository>();
        _outboxService = Substitute.For<IOutboxEventService>();
        _userContext = Substitute.For<IUserContext>();
        _userContext.UserId.Returns(UserId);

        var queuesConfig = new MessageQueuesConfiguration();

        _handler = new UpdateCartItemQuantityCommandHandler(
            _cartRepository,
            _warehouseRepository,
            _outboxService,
            queuesConfig,
            _userContext);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenIncreasingQuantityAndStockAvailable_ShouldUpdateQuantity()
    {
        // Arrange
        var command = new UpdateCartItemQuantityCommand(ProductId, 5);
        var cartItem = new CartItem { Id = CartItemId, ProductId = ProductId, Quantity = 2 };
        var cart = new Cart { UserId = UserId, CartItems = new List<CartItem> { cartItem } };
        var warehouseItem = new WarehouseItem { ProductId = ProductId, Quantity = 10 };

        _cartRepository.GetCartByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(cart);
        _warehouseRepository.GetWarehouseItemByProductIdAsync(ProductId, Arg.Any<CancellationToken>()).Returns(warehouseItem);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(5, cartItem.Quantity);
        Assert.Equal(7, warehouseItem.Quantity);
        await _cartRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenDecreasingQuantity_ShouldUpdateQuantity()
    {
        // Arrange
        var command = new UpdateCartItemQuantityCommand(ProductId, 1);
        var cartItem = new CartItem { Id = CartItemId, ProductId = ProductId, Quantity = 3 };
        var cart = new Cart { UserId = UserId, CartItems = new List<CartItem> { cartItem } };
        var warehouseItem = new WarehouseItem { ProductId = ProductId, Quantity = 10 };

        _cartRepository.GetCartByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(cart);
        _warehouseRepository.GetWarehouseItemByProductIdAsync(ProductId, Arg.Any<CancellationToken>()).Returns(warehouseItem);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(1, cartItem.Quantity);
        Assert.Equal(12, warehouseItem.Quantity);
        await _cartRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenQuantitySetToZero_ShouldRemoveItem()
    {
        // Arrange
        var command = new UpdateCartItemQuantityCommand(ProductId, 0);
        var cartItem = new CartItem { Id = CartItemId, ProductId = ProductId, Quantity = 2 };
        var cart = new Cart { UserId = UserId, CartItems = new List<CartItem> { cartItem } };
        var warehouseItem = new WarehouseItem { ProductId = ProductId, Quantity = 10 };

        _cartRepository.GetCartByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(cart);
        _warehouseRepository.GetWarehouseItemByProductIdAsync(ProductId, Arg.Any<CancellationToken>()).Returns(warehouseItem);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        await _cartRepository.Received(1).RemoveCartItemAsync(CartItemId, Arg.Any<CancellationToken>());
        Assert.Equal(12, warehouseItem.Quantity);
        await _cartRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenNotEnoughStock_ShouldReturnFailedResultWithBadRequestError()
    {
        // Arrange
        var command = new UpdateCartItemQuantityCommand(ProductId, 10);
        var cartItem = new CartItem { Id = CartItemId, ProductId = ProductId, Quantity = 2 };
        var cart = new Cart { UserId = UserId, CartItems = new List<CartItem> { cartItem } };
        var warehouseItem = new WarehouseItem { ProductId = ProductId, Quantity = 5 };

        _cartRepository.GetCartByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(cart);
        _warehouseRepository.GetWarehouseItemByProductIdAsync(ProductId, Arg.Any<CancellationToken>()).Returns(warehouseItem);

        // Act & Assert
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<BadRequestError>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenIncreasingQuantity_ShouldRestartTheReservation()
    {
        // Arrange
        DateTime originalReservedAt = DateTime.UtcNow.AddHours(-3);

        var command = new UpdateCartItemQuantityCommand(ProductId, 5);
        var cartItem = new CartItem
        {
            Id = CartItemId,
            ProductId = ProductId,
            Quantity = 2,
            ReservedAt = originalReservedAt
        };
        var cart = new Cart { UserId = UserId, CartItems = new List<CartItem> { cartItem } };
        var warehouseItem = new WarehouseItem { ProductId = ProductId, Quantity = 10 };

        _cartRepository.GetCartByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(cart);
        _warehouseRepository.GetWarehouseItemByProductIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(warehouseItem);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.True(cartItem.ReservedAt > originalReservedAt);
        Assert.True(cartItem.ReservedAt > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenDecreasingQuantity_ShouldNotRestartTheReservation()
    {
        // Arrange
        DateTime originalReservedAt = DateTime.UtcNow.AddHours(-3);

        var command = new UpdateCartItemQuantityCommand(ProductId, 1);
        var cartItem = new CartItem
        {
            Id = CartItemId,
            ProductId = ProductId,
            Quantity = 4,
            ReservedAt = originalReservedAt
        };
        var cart = new Cart { UserId = UserId, CartItems = new List<CartItem> { cartItem } };
        var warehouseItem = new WarehouseItem { ProductId = ProductId, Quantity = 10 };

        _cartRepository.GetCartByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(cart);
        _warehouseRepository.GetWarehouseItemByProductIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(warehouseItem);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(originalReservedAt, cartItem.ReservedAt);
    }
}

