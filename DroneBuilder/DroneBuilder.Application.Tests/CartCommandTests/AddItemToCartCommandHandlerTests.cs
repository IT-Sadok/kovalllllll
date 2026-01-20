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

public class AddItemToCartCommandHandlerTests
{
    private readonly ICartRepository _cartRepository;
    private readonly IOutboxEventService _outboxService;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IProductRepository _productRepository;
    private readonly AddItemToCartCommandHandler _handler;

    private const string CartQueueName = "cart-queue";
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ProductId = Guid.NewGuid();
    private const string ProductName = "Test Product";
    private const int ValidQuantity = 5;
    private const int WarehouseQuantity = 100;

    public AddItemToCartCommandHandlerTests()
    {
        // Arrange
        _cartRepository = Substitute.For<ICartRepository>();
        _outboxService = Substitute.For<IOutboxEventService>();
        _warehouseRepository = Substitute.For<IWarehouseRepository>();
        _productRepository = Substitute.For<IProductRepository>();
        var userContext = Substitute.For<IUserContext>();

        var queuesConfig = new MessageQueuesConfiguration
        {
            CartQueue = new QueueConfiguration { Name = CartQueueName }
        };

        userContext.UserId.Returns(UserId);

        _handler = new AddItemToCartCommandHandler(
            _cartRepository,
            _outboxService,
            _warehouseRepository,
            _productRepository,
            queuesConfig,
            userContext);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenAddingNewItemToExistingCart_ShouldAddItemAndUpdateWarehouse()
    {
        // Arrange
        var command = new AddItemToCartCommand(ProductId, ValidQuantity);

        var product = new Product
        {
            Id = ProductId,
            Name = ProductName
        };

        var warehouseItem = new WarehouseItem
        {
            ProductId = ProductId,
            Quantity = WarehouseQuantity
        };

        var existingCart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            CartItems = new List<CartItem>()
        };

        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(product);

        _warehouseRepository.GetWarehouseItemByProductIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(warehouseItem);

        _cartRepository.GetCartByUserIdAsync(UserId, Arg.Any<CancellationToken>())
            .Returns(existingCart);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        await _cartRepository.Received(1).AddCartItemAsync(
            Arg.Is<CartItem>(ci =>
                ci.ProductId == ProductId &&
                ci.ProductName == ProductName &&
                ci.Quantity == ValidQuantity),
            Arg.Any<CancellationToken>());

        Assert.Equal(WarehouseQuantity - ValidQuantity, warehouseItem.Quantity);

        await _outboxService.Received(1).StoreEventAsync(
            Arg.Is<AddedItemToCartEvent>(e =>
                e.UserId == UserId &&
                e.ProductId == ProductId &&
                e.ProductName == ProductName &&
                e.Quantity == ValidQuantity),
            CartQueueName,
            Arg.Any<CancellationToken>());

        await _cartRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenItemAlreadyInCart_ShouldIncreaseQuantity()
    {
        // Arrange
        var command = new AddItemToCartCommand(ProductId, ValidQuantity);
        var initialCartItemQuantity = 3;

        var product = new Product
        {
            Id = ProductId,
            Name = ProductName
        };

        var warehouseItem = new WarehouseItem
        {
            ProductId = ProductId,
            Quantity = WarehouseQuantity
        };

        var existingCartItem = new CartItem
        {
            ProductId = ProductId,
            ProductName = ProductName,
            Quantity = initialCartItemQuantity
        };

        var existingCart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            CartItems = new List<CartItem> { existingCartItem }
        };

        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(product);

        _warehouseRepository.GetWarehouseItemByProductIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(warehouseItem);

        _cartRepository.GetCartByUserIdAsync(UserId, Arg.Any<CancellationToken>())
            .Returns(existingCart);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(initialCartItemQuantity + ValidQuantity, existingCartItem.Quantity);

        await _cartRepository.DidNotReceive().AddCartItemAsync(
            Arg.Is<CartItem>(ci =>
                ci.ProductId == ProductId &&
                ci.ProductName == ProductName &&
                ci.Quantity == ValidQuantity),
            Arg.Any<CancellationToken>());

        Assert.Equal(WarehouseQuantity - ValidQuantity, warehouseItem.Quantity);

        await _cartRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenCartDoesNotExist_ShouldCreateNewCart()
    {
        // Arrange
        var command = new AddItemToCartCommand(ProductId, ValidQuantity);

        var product = new Product
        {
            Id = ProductId,
            Name = ProductName
        };

        var warehouseItem = new WarehouseItem
        {
            ProductId = ProductId,
            Quantity = WarehouseQuantity
        };

        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(product);

        _warehouseRepository.GetWarehouseItemByProductIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(warehouseItem);

        _cartRepository.GetCartByUserIdAsync(UserId, Arg.Any<CancellationToken>())
            .Returns((Cart)null);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        await _cartRepository.Received(1).CreateCartAsync(
            Arg.Is<Cart>(c =>
                c.UserId == UserId &&
                c.CartItems != null),
            Arg.Any<CancellationToken>());

        await _cartRepository.Received(1).AddCartItemAsync(
            Arg.Is<CartItem>(ci =>
                ci.ProductId == ProductId &&
                ci.Quantity == ValidQuantity),
            Arg.Any<CancellationToken>());

        await _cartRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new AddItemToCartCommand(ProductId, ValidQuantity);

        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns((Product)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal($"Product with ID {ProductId} not found.", exception.Message);

        await _warehouseRepository.DidNotReceive().GetWarehouseItemByProductIdAsync(
            Arg.Is<Guid>(id => id == ProductId),
            Arg.Any<CancellationToken>());

        await _cartRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenWarehouseItemNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new AddItemToCartCommand(ProductId, ValidQuantity);

        var product = new Product
        {
            Id = ProductId,
            Name = ProductName
        };

        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(product);

        _warehouseRepository.GetWarehouseItemByProductIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns((WarehouseItem)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal($"Warehouse item for product ID {ProductId} not found.", exception.Message);

        await _cartRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenQuantityIsZero_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new AddItemToCartCommand(ProductId, 0);

        var product = new Product
        {
            Id = ProductId,
            Name = ProductName
        };

        var warehouseItem = new WarehouseItem
        {
            ProductId = ProductId,
            Quantity = WarehouseQuantity
        };

        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(product);

        _warehouseRepository.GetWarehouseItemByProductIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(warehouseItem);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal("Quantity must be greater than zero.", exception.Message);

        await _cartRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenQuantityIsNegative_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new AddItemToCartCommand(ProductId, -5);

        var product = new Product
        {
            Id = ProductId,
            Name = ProductName
        };

        var warehouseItem = new WarehouseItem
        {
            ProductId = ProductId,
            Quantity = WarehouseQuantity
        };

        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(product);

        _warehouseRepository.GetWarehouseItemByProductIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(warehouseItem);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal("Quantity must be greater than zero.", exception.Message);
    }
}