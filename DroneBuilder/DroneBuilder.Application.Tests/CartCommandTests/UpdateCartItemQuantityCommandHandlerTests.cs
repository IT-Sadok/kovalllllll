using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Features.Cart.UpdateCartItemQuantity;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Application.Tests.TestSupport;
using DroneBuilder.Domain.Entities;
using FluentResults;
using NSubstitute;

namespace DroneBuilder.Application.Tests.CartCommandTests;

public class UpdateCartItemQuantityCommandHandlerTests
{
    private readonly ICartRepository _cartRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IUserContext _userContext;
    private readonly UpdateCartItemQuantityCommandHandler _handler;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ProductId = Guid.NewGuid();
    private static readonly Guid CartItemId = Guid.NewGuid();

    public UpdateCartItemQuantityCommandHandlerTests()
    {
        _cartRepository = Substitute.For<ICartRepository>();
        _warehouseRepository = Substitute.For<IWarehouseRepository>();
        _userContext = Substitute.For<IUserContext>();
        _userContext.UserId.Returns(UserId);

        _handler = new UpdateCartItemQuantityCommandHandler(
            _cartRepository,
            _warehouseRepository,
            _userContext);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenIncreasingQuantityAndStockAvailable_ShouldUpdateQuantity()
    {
        // Arrange
        var command = new UpdateCartItemQuantityCommand(ProductId, 5);
        var cartItem = new CartItem { Id = CartItemId, ProductId = ProductId, Quantity = 2 };
        var cart = new Cart { UserId = UserId, CartItems = new List<CartItem> { cartItem } };
        var warehouseItem = new WarehouseItem { ProductId = ProductId, Quantity = 10, ReservedQuantity = 2 };
        ReservationTestData.Attach(cart, cartItem, warehouseItem);

        _cartRepository.GetCartByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(cart);
        _warehouseRepository.GetWarehouseItemByProductIdAsync(ProductId, Arg.Any<CancellationToken>()).Returns(warehouseItem);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(5, cartItem.Quantity);
        Assert.Equal(10, warehouseItem.Quantity);
        Assert.Equal(5, warehouseItem.ReservedQuantity);
        await _cartRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenDecreasingQuantity_ShouldUpdateQuantity()
    {
        // Arrange
        var command = new UpdateCartItemQuantityCommand(ProductId, 1);
        var cartItem = new CartItem { Id = CartItemId, ProductId = ProductId, Quantity = 3 };
        var cart = new Cart { UserId = UserId, CartItems = new List<CartItem> { cartItem } };
        var warehouseItem = new WarehouseItem { ProductId = ProductId, Quantity = 10, ReservedQuantity = 3 };
        ReservationTestData.Attach(cart, cartItem, warehouseItem);

        _cartRepository.GetCartByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(cart);
        _warehouseRepository.GetWarehouseItemByProductIdAsync(ProductId, Arg.Any<CancellationToken>()).Returns(warehouseItem);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(1, cartItem.Quantity);
        Assert.Equal(10, warehouseItem.Quantity);
        Assert.Equal(1, warehouseItem.ReservedQuantity);
        await _cartRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenQuantitySetToZero_ShouldRemoveItem()
    {
        // Arrange
        var command = new UpdateCartItemQuantityCommand(ProductId, 0);
        var cartItem = new CartItem { Id = CartItemId, ProductId = ProductId, Quantity = 2 };
        var cart = new Cart { UserId = UserId, CartItems = new List<CartItem> { cartItem } };
        var warehouseItem = new WarehouseItem { ProductId = ProductId, Quantity = 10, ReservedQuantity = 2 };
        ReservationTestData.Attach(cart, cartItem, warehouseItem);

        _cartRepository.GetCartByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(cart);
        _warehouseRepository.GetWarehouseItemByProductIdAsync(ProductId, Arg.Any<CancellationToken>()).Returns(warehouseItem);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        await _cartRepository.Received(1).RemoveCartItemAsync(CartItemId, Arg.Any<CancellationToken>());
        Assert.Equal(10, warehouseItem.Quantity);
        Assert.Equal(0, warehouseItem.ReservedQuantity);
        await _cartRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenNotEnoughStock_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new UpdateCartItemQuantityCommand(ProductId, 10);
        var cartItem = new CartItem { Id = CartItemId, ProductId = ProductId, Quantity = 2 };
        var cart = new Cart { UserId = UserId, CartItems = new List<CartItem> { cartItem } };
        var warehouseItem = new WarehouseItem
        {
            ProductId = ProductId,
            Quantity = 5,
            ReservedQuantity = 2
        }; // Only 3 available, need 8 more
        ReservationTestData.Attach(cart, cartItem, warehouseItem);

        _cartRepository.GetCartByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(cart);
        _warehouseRepository.GetWarehouseItemByProductIdAsync(ProductId, Arg.Any<CancellationToken>()).Returns(warehouseItem);

        // Act & Assert
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<BadRequestError>());
    }
}

