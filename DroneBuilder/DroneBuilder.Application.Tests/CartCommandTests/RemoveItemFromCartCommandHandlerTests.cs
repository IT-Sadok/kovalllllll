using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Commands.CartCommands;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using NSubstitute;

namespace DroneBuilder.Application.Tests.CartCommandTests;

public class RemoveItemFromCartCommandHandlerTests
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly RemoveItemFromCartCommandHandler _handler;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid CartId = Guid.NewGuid();
    private static readonly Guid ProductId = Guid.NewGuid();
    private static readonly Guid CartItemId = Guid.NewGuid();
    private const string ProductName = "Test Product";
    private const string OtherProductName = "Other Product";
    private const int CartItemQuantity = 5;
    private const int WarehouseQuantity = 100;

    public RemoveItemFromCartCommandHandlerTests()
    {
        // Arrange
        _cartRepository = Substitute.For<ICartRepository>();
        _productRepository = Substitute.For<IProductRepository>();
        _warehouseRepository = Substitute.For<IWarehouseRepository>();
        var userContext = Substitute.For<IUserContext>();

        userContext.UserId.Returns(UserId);

        _handler = new RemoveItemFromCartCommandHandler(
            _cartRepository,
            _productRepository,
            _warehouseRepository,
            userContext);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenItemExists_ShouldRemoveItemAndReturnToWarehouse()
    {
        // Arrange
        var command = new RemoveItemFromCartCommand(ProductId);

        var product = new Product
        {
            Id = ProductId,
            Name = ProductName
        };

        var cartItem = new CartItem
        {
            Id = CartItemId,
            ProductId = ProductId,
            ProductName = ProductName,
            Quantity = CartItemQuantity
        };

        var cart = new Cart
        {
            Id = CartId,
            UserId = UserId,
            CartItems = new List<CartItem> { cartItem }
        };

        var warehouseItem = new WarehouseItem
        {
            ProductId = ProductId,
            Quantity = WarehouseQuantity
        };

        _cartRepository.GetCartByUserIdAsync(UserId, Arg.Any<CancellationToken>())
            .Returns(cart);

        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(product);

        _warehouseRepository.GetWarehouseItemByProductIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(warehouseItem);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(WarehouseQuantity + CartItemQuantity, warehouseItem.Quantity);

        await _cartRepository.Received(1).RemoveCartItemAsync(CartItemId, Arg.Any<CancellationToken>());
        await _cartRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenCartNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new RemoveItemFromCartCommand(ProductId);

        _cartRepository.GetCartByUserIdAsync(UserId, Arg.Any<CancellationToken>())
            .Returns((Cart)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal($"Cart for User ID {UserId} not found.", exception.Message);

        await _productRepository.DidNotReceive().GetProductByIdAsync(
            Arg.Is<Guid>(id => id == ProductId),
            Arg.Any<CancellationToken>());

        await _cartRepository.DidNotReceive().RemoveCartItemAsync(
            Arg.Is<Guid>(id => id == CartItemId),
            Arg.Any<CancellationToken>());

        await _cartRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new RemoveItemFromCartCommand(ProductId);

        var cart = new Cart
        {
            Id = CartId,
            UserId = UserId,
            CartItems = new List<CartItem>()
        };

        _cartRepository.GetCartByUserIdAsync(UserId, Arg.Any<CancellationToken>())
            .Returns(cart);

        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns((Product)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal($"Product with ID {ProductId} not found.", exception.Message);

        await _cartRepository.DidNotReceive().RemoveCartItemAsync(
            Arg.Is<Guid>(id => id == CartItemId),
            Arg.Any<CancellationToken>());

        await _cartRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenItemNotInCart_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new RemoveItemFromCartCommand(ProductId);

        var product = new Product
        {
            Id = ProductId,
            Name = ProductName
        };

        var otherCartItem = new CartItem
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = OtherProductName,
            Quantity = 3
        };

        var cart = new Cart
        {
            Id = CartId,
            UserId = UserId,
            CartItems = new List<CartItem> { otherCartItem }
        };

        _cartRepository.GetCartByUserIdAsync(UserId, Arg.Any<CancellationToken>())
            .Returns(cart);

        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(product);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal($"Product with ID {ProductId} not found in the cart.", exception.Message);

        await _warehouseRepository.DidNotReceive().GetWarehouseItemByProductIdAsync(
            Arg.Is<Guid>(id => id == ProductId),
            Arg.Any<CancellationToken>());

        await _cartRepository.DidNotReceive().RemoveCartItemAsync(
            Arg.Is<Guid>(id => id == CartItemId),
            Arg.Any<CancellationToken>());

        await _cartRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenWarehouseItemNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new RemoveItemFromCartCommand(ProductId);

        var product = new Product
        {
            Id = ProductId,
            Name = ProductName
        };

        var cartItem = new CartItem
        {
            Id = CartItemId,
            ProductId = ProductId,
            ProductName = ProductName,
            Quantity = CartItemQuantity
        };

        var cart = new Cart
        {
            Id = CartId,
            UserId = UserId,
            CartItems = new List<CartItem> { cartItem }
        };

        _cartRepository.GetCartByUserIdAsync(UserId, Arg.Any<CancellationToken>())
            .Returns(cart);

        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(product);

        _warehouseRepository.GetWarehouseItemByProductIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns((WarehouseItem)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal($"Warehouse item for Product ID {ProductId} not found.", exception.Message);

        await _cartRepository.DidNotReceive().RemoveCartItemAsync(
            Arg.Is<Guid>(id => id == CartItemId),
            Arg.Any<CancellationToken>());

        await _cartRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenMultipleItemsInCart_ShouldRemoveOnlySpecifiedItem()
    {
        // Arrange
        var command = new RemoveItemFromCartCommand(ProductId);

        var product = new Product
        {
            Id = ProductId,
            Name = ProductName
        };

        var cartItemToRemove = new CartItem
        {
            Id = CartItemId,
            ProductId = ProductId,
            ProductName = ProductName,
            Quantity = CartItemQuantity
        };

        var otherCartItem = new CartItem
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = OtherProductName,
            Quantity = 10
        };

        var cart = new Cart
        {
            Id = CartId,
            UserId = UserId,
            CartItems = new List<CartItem> { cartItemToRemove, otherCartItem }
        };

        var warehouseItem = new WarehouseItem
        {
            ProductId = ProductId,
            Quantity = WarehouseQuantity
        };

        _cartRepository.GetCartByUserIdAsync(UserId, Arg.Any<CancellationToken>())
            .Returns(cart);

        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(product);

        _warehouseRepository.GetWarehouseItemByProductIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(warehouseItem);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        await _cartRepository.Received(1).RemoveCartItemAsync(CartItemId, Arg.Any<CancellationToken>());

        await _cartRepository.Received(1).RemoveCartItemAsync(
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenSuccessful_ShouldUpdateWarehouseCorrectly()
    {
        // Arrange
        var command = new RemoveItemFromCartCommand(ProductId);
        const int initialWarehouseQuantity = 50;
        const int cartQuantity = 15;

        var product = new Product
        {
            Id = ProductId,
            Name = ProductName
        };

        var cartItem = new CartItem
        {
            Id = CartItemId,
            ProductId = ProductId,
            ProductName = ProductName,
            Quantity = cartQuantity
        };

        var cart = new Cart
        {
            Id = CartId,
            UserId = UserId,
            CartItems = new List<CartItem> { cartItem }
        };

        var warehouseItem = new WarehouseItem
        {
            ProductId = ProductId,
            Quantity = initialWarehouseQuantity
        };

        _cartRepository.GetCartByUserIdAsync(UserId, Arg.Any<CancellationToken>())
            .Returns(cart);

        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(product);

        _warehouseRepository.GetWarehouseItemByProductIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(warehouseItem);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(initialWarehouseQuantity + cartQuantity, warehouseItem.Quantity);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenEmptyCart_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new RemoveItemFromCartCommand(ProductId);

        var product = new Product
        {
            Id = ProductId,
            Name = ProductName
        };

        var cart = new Cart
        {
            Id = CartId,
            UserId = UserId,
            CartItems = new List<CartItem>()
        };

        _cartRepository.GetCartByUserIdAsync(UserId, Arg.Any<CancellationToken>())
            .Returns(cart);

        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(product);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal($"Product with ID {ProductId} not found in the cart.", exception.Message);

        await _cartRepository.DidNotReceive().RemoveCartItemAsync(
            Arg.Is<Guid>(id => id == CartItemId),
            Arg.Any<CancellationToken>());
    }
}