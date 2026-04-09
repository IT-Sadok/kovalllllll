using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Commands.CartCommands;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using NSubstitute;
using Xunit;

namespace DroneBuilder.Application.Tests.CartCommandTests;

public class UpdateCartItemQuantityCommandHandlerTests
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
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
        _productRepository = Substitute.For<IProductRepository>();
        _warehouseRepository = Substitute.For<IWarehouseRepository>();
        _outboxService = Substitute.For<IOutboxEventService>();
        _userContext = Substitute.For<IUserContext>();
        _userContext.UserId.Returns(UserId);

        var queuesConfig = new MessageQueuesConfiguration();

        _handler = new UpdateCartItemQuantityCommandHandler(
            _cartRepository,
            _productRepository,
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
        Assert.Equal(7, warehouseItem.Quantity); // 10 - (5-2) = 7
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
        Assert.Equal(12, warehouseItem.Quantity); // 10 - (1-3) = 12
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
        Assert.Equal(12, warehouseItem.Quantity); // 10 - (0-2) = 12
        await _cartRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenNotEnoughStock_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new UpdateCartItemQuantityCommand(ProductId, 10);
        var cartItem = new CartItem { Id = CartItemId, ProductId = ProductId, Quantity = 2 };
        var cart = new Cart { UserId = UserId, CartItems = new List<CartItem> { cartItem } };
        var warehouseItem = new WarehouseItem { ProductId = ProductId, Quantity = 5 }; // Only 5 available, need 8 more

        _cartRepository.GetCartByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(cart);
        _warehouseRepository.GetWarehouseItemByProductIdAsync(ProductId, Arg.Any<CancellationToken>()).Returns(warehouseItem);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.ExecuteCommandAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenNegativeQuantity_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new UpdateCartItemQuantityCommand(ProductId, -1);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _handler.ExecuteCommandAsync(command, CancellationToken.None));
    }
}
