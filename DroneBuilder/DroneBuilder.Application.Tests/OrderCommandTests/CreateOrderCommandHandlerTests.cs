using System.Text.Json;
using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Commands.OrderCommands;
using DroneBuilder.Application.Models.OrderModels;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.OrderEvents;
using MapsterMapper;
using NSubstitute;

namespace DroneBuilder.Application.Tests.OrderCommandTests;

public class CreateOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IOutboxEventService _outboxService;
    private readonly IMapper _mapper;
    private readonly CreateOrderCommandHandler _handler;

    private const string OrderQueueName = "order-queue";
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid CartId = Guid.NewGuid();
    private static readonly Guid ProductId1 = Guid.NewGuid();
    private static readonly Guid ProductId2 = Guid.NewGuid();
    private const decimal Product1Price = 100.50m;
    private const decimal Product2Price = 50.25m;
    private const int Product1Quantity = 2;
    private const int Product2Quantity = 3;

    public CreateOrderCommandHandlerTests()
    {
        // Arrange
        _orderRepository = Substitute.For<IOrderRepository>();
        _cartRepository = Substitute.For<ICartRepository>();
        _productRepository = Substitute.For<IProductRepository>();
        _warehouseRepository = Substitute.For<IWarehouseRepository>();
        _outboxService = Substitute.For<IOutboxEventService>();
        var userContext = Substitute.For<IUserContext>();
        _mapper = Substitute.For<IMapper>();

        var queuesConfig = new MessageQueuesConfiguration
        {
            OrderQueue = new QueueConfiguration { Name = OrderQueueName }
        };

        userContext.UserId.Returns(UserId);

        _handler = new CreateOrderCommandHandler(
            _orderRepository,
            _cartRepository,
            _productRepository,
            _warehouseRepository,
            _outboxService,
            queuesConfig,
            userContext,
            _mapper);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenCartHasItems_ShouldCreateOrderSuccessfully()
    {
        // Arrange
        var shippingDetails = new ShippingDetailsModel
        {
            FullName = "Іван Коваленко",
            AddressLine1 = "вул. Хрещатик, 123",
            AddressLine2 = "кв. 45",
            City = "Київ",
            State = "Київська область",
            PostalCode = "01001",
            Country = "Україна",
            PhoneNumber = "+380501234567"
        };
        var command = new CreateOrderCommand(shippingDetails);

        var cartItem1 = new CartItem
        {
            ProductId = ProductId1,
            Quantity = Product1Quantity
        };

        var cartItem2 = new CartItem
        {
            ProductId = ProductId2,
            Quantity = Product2Quantity
        };

        var cart = new Cart
        {
            Id = CartId,
            UserId = UserId,
            CartItems = new List<CartItem> { cartItem1, cartItem2 }
        };

        var product1 = new Product { Id = ProductId1, Price = Product1Price };
        var product2 = new Product { Id = ProductId2, Price = Product2Price };
        var products = new List<Product> { product1, product2 };

        var warehouseItems = new List<WarehouseItem>
        {
            new() { ProductId = ProductId1, Quantity = 100 },
            new() { ProductId = ProductId2, Quantity = 100 }
        };

        const decimal expectedTotalPrice = (Product1Price * Product1Quantity) + (Product2Price * Product2Quantity);
        var expectedOrderModel = new OrderModel { TotalPrice = expectedTotalPrice };

        _cartRepository.GetCartByUserIdAsync(
                Arg.Is<Guid>(id => id == UserId),
                Arg.Any<CancellationToken>())
            .Returns(cart);

        _warehouseRepository.GetAllWarehouseItemsByProductIdsAsync(
                Arg.Is<List<Guid>>(ids => ids.Contains(ProductId1) && ids.Contains(ProductId2)),
                Arg.Any<CancellationToken>())
            .Returns(warehouseItems);

        _productRepository.GetProductsByIdsAsync(
                Arg.Is<List<Guid>>(ids => ids.Contains(ProductId1) && ids.Contains(ProductId2)),
                Arg.Any<CancellationToken>())
            .Returns(products);

        _mapper.Map<OrderModel>(Arg.Is<Order>(o =>
                o.UserId == UserId &&
                o.TotalPrice == expectedTotalPrice &&
                o.OrderItems.Count == 2))
            .Returns(expectedOrderModel);

        // Act
        var result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedTotalPrice, result.TotalPrice);

        await _orderRepository.Received(1).CreateOrderAsync(
            Arg.Is<Order>(o =>
                o.UserId == UserId &&
                o.TotalPrice == expectedTotalPrice &&
                o.OrderItems.Count == 2),
            Arg.Any<CancellationToken>());

        await _cartRepository.Received(1).ClearCartAsync(
            Arg.Is<Guid>(id => id == CartId),
            Arg.Any<CancellationToken>());

        await _outboxService.Received(1).StoreEventAsync(
            Arg.Is<OrderCreatedEvent>(e => e.UserId == UserId),
            Arg.Is<string>(q => q == OrderQueueName),
            Arg.Any<CancellationToken>());

        await _orderRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenCartIsEmpty_ShouldThrowBadRequestException()
    {
        // Arrange
        var shippingDetails = new ShippingDetailsModel
        {
            FullName = "Test User",
            AddressLine1 = "Test Address",
            City = "Test City",
            Country = "Test Country"
        };
        var command = new CreateOrderCommand(shippingDetails);

        var cart = new Cart
        {
            Id = CartId,
            UserId = UserId,
            CartItems = new List<CartItem>()
        };

        _cartRepository.GetCartByUserIdAsync(
                Arg.Is<Guid>(id => id == UserId),
                Arg.Any<CancellationToken>())
            .Returns(cart);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal("Cart is empty.", exception.Message);

        await _orderRepository.DidNotReceive().CreateOrderAsync(
            Arg.Is<Order>(o => o.UserId == UserId),
            Arg.Any<CancellationToken>());

        await _cartRepository.DidNotReceive().ClearCartAsync(
            Arg.Is<Guid>(id => id == CartId),
            Arg.Any<CancellationToken>());

        await _outboxService.DidNotReceive().StoreEventAsync(
            Arg.Is<OrderCreatedEvent>(e => e.UserId == UserId),
            Arg.Is<string>(q => q == OrderQueueName),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenCartNotFound_ShouldThrowBadRequestException()
    {
        // Arrange
        var shippingDetails = new ShippingDetailsModel
        {
            FullName = "Test User",
            AddressLine1 = "Test Address",
            City = "Test City",
            Country = "Test Country"
        };
        var command = new CreateOrderCommand(shippingDetails);

        _cartRepository.GetCartByUserIdAsync(
                Arg.Is<Guid>(id => id == UserId),
                Arg.Any<CancellationToken>())
            .Returns((Cart)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal("Cart is empty.", exception.Message);

        await _productRepository.DidNotReceive().GetProductsByIdsAsync(
            Arg.Is<List<Guid>>(ids => ids.Contains(ProductId1) && ids.Contains(ProductId2)),
            Arg.Any<CancellationToken>());

        await _orderRepository.DidNotReceive().CreateOrderAsync(
            Arg.Is<Order>(o => o.UserId == UserId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenProductNotInWarehouse_ShouldThrowNotFoundException()
    {
        // Arrange
        var shippingDetails = new ShippingDetailsModel
        {
            FullName = "Test User",
            AddressLine1 = "Test Address",
            City = "Test City",
            Country = "Test Country"
        };
        var command = new CreateOrderCommand(shippingDetails);

        var cartItem = new CartItem
        {
            ProductId = ProductId1,
            Quantity = 1
        };

        var cart = new Cart
        {
            Id = CartId,
            UserId = UserId,
            CartItems = new List<CartItem> { cartItem }
        };

        _cartRepository.GetCartByUserIdAsync(
                Arg.Is<Guid>(id => id == UserId),
                Arg.Any<CancellationToken>())
            .Returns(cart);

        _warehouseRepository.GetAllWarehouseItemsByProductIdsAsync(
                Arg.Is<List<Guid>>(ids => ids.Contains(ProductId1)),
                Arg.Any<CancellationToken>())
            .Returns((List<WarehouseItem>)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Contains($"Product {ProductId1} not found in warehouse.", exception.Message);

        await _orderRepository.DidNotReceive().CreateOrderAsync(
            Arg.Any<Order>(),
            Arg.Any<CancellationToken>());

        await _cartRepository.DidNotReceive().ClearCartAsync(
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenSuccessful_ShouldCalculateTotalPriceCorrectly()
    {
        // Arrange
        var shippingDetails = new ShippingDetailsModel
        {
            FullName = "Test User",
            AddressLine1 = "Test Address",
            City = "Test City",
            Country = "Test Country"
        };
        var command = new CreateOrderCommand(shippingDetails);

        var cartItem = new CartItem
        {
            ProductId = ProductId1,
            Quantity = 3
        };

        var cart = new Cart
        {
            Id = CartId,
            UserId = UserId,
            CartItems = new List<CartItem> { cartItem }
        };

        var product = new Product { Id = ProductId1, Price = 99.99m };
        var products = new List<Product> { product };

        var warehouseItems = new List<WarehouseItem>
        {
            new() { ProductId = ProductId1, Quantity = 100 }
        };

        _cartRepository.GetCartByUserIdAsync(
                Arg.Is<Guid>(id => id == UserId),
                Arg.Any<CancellationToken>())
            .Returns(cart);

        _warehouseRepository.GetAllWarehouseItemsByProductIdsAsync(
                Arg.Is<List<Guid>>(ids => ids.Contains(ProductId1)),
                Arg.Any<CancellationToken>())
            .Returns(warehouseItems);

        _productRepository.GetProductsByIdsAsync(
                Arg.Is<List<Guid>>(ids => ids.Contains(ProductId1)),
                Arg.Any<CancellationToken>())
            .Returns(products);

        _mapper.Map<OrderModel>(Arg.Any<Order>())
            .Returns(new OrderModel());

        Order capturedOrder = null;
        await _orderRepository.CreateOrderAsync(
            Arg.Do<Order>(o => capturedOrder = o),
            Arg.Any<CancellationToken>());

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedOrder);
        Assert.Equal(299.97m, capturedOrder.TotalPrice);
        Assert.Single(capturedOrder.OrderItems);
        Assert.Equal(99.99m, capturedOrder.OrderItems.First().PriceAtPurchase);
        Assert.Equal(3, capturedOrder.OrderItems.First().Quantity);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenSuccessful_ShouldSerializeShippingDetails()
    {
        // Arrange
        var shippingDetails = new ShippingDetailsModel
        {
            FullName = "Олександр Петренко",
            AddressLine1 = "вул. Шевченка, 50",
            AddressLine2 = "офіс 12",
            City = "Львів",
            State = "Львівська область",
            PostalCode = "79000",
            Country = "Україна",
            PhoneNumber = "+380671234567"
        };
        var command = new CreateOrderCommand(shippingDetails);

        var cartItem = new CartItem { ProductId = ProductId1, Quantity = 1 };
        var cart = new Cart
        {
            Id = CartId,
            UserId = UserId,
            CartItems = new List<CartItem> { cartItem }
        };

        var product = new Product { Id = ProductId1, Price = 100m };
        var products = new List<Product> { product };

        var warehouseItems = new List<WarehouseItem>
        {
            new() { ProductId = ProductId1, Quantity = 100 }
        };

        _cartRepository.GetCartByUserIdAsync(
                Arg.Is<Guid>(id => id == UserId),
                Arg.Any<CancellationToken>())
            .Returns(cart);

        _warehouseRepository.GetAllWarehouseItemsByProductIdsAsync(
                Arg.Is<List<Guid>>(ids => ids.Contains(ProductId1)),
                Arg.Any<CancellationToken>())
            .Returns(warehouseItems);

        _productRepository.GetProductsByIdsAsync(
                Arg.Is<List<Guid>>(ids => ids.Contains(ProductId1)),
                Arg.Any<CancellationToken>())
            .Returns(products);

        _mapper.Map<OrderModel>(Arg.Any<Order>())
            .Returns(new OrderModel());

        Order capturedOrder = null;
        await _orderRepository.CreateOrderAsync(
            Arg.Do<Order>(o => capturedOrder = o),
            Arg.Any<CancellationToken>());

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedOrder);
        Assert.NotNull(capturedOrder.ShippingDetails);

        var deserializedDetails = JsonSerializer.Deserialize<ShippingDetailsModel>(
            capturedOrder.ShippingDetails);

        Assert.NotNull(deserializedDetails);
        Assert.Equal(shippingDetails.FullName, deserializedDetails.FullName);
        Assert.Equal(shippingDetails.AddressLine1, deserializedDetails.AddressLine1);
        Assert.Equal(shippingDetails.AddressLine2, deserializedDetails.AddressLine2);
        Assert.Equal(shippingDetails.City, deserializedDetails.City);
        Assert.Equal(shippingDetails.State, deserializedDetails.State);
        Assert.Equal(shippingDetails.PostalCode, deserializedDetails.PostalCode);
        Assert.Equal(shippingDetails.Country, deserializedDetails.Country);
        Assert.Equal(shippingDetails.PhoneNumber, deserializedDetails.PhoneNumber);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenSuccessful_ShouldGenerateCorrectEvent()
    {
        // Arrange
        var shippingDetails = new ShippingDetailsModel
        {
            FullName = "Test User",
            AddressLine1 = "Test Address",
            City = "Test City",
            Country = "Test Country"
        };
        var command = new CreateOrderCommand(shippingDetails);

        var cartItem = new CartItem { ProductId = ProductId1, Quantity = 1 };
        var cart = new Cart
        {
            Id = CartId,
            UserId = UserId,
            CartItems = new List<CartItem> { cartItem }
        };

        var product = new Product { Id = ProductId1, Price = 100m };
        var products = new List<Product> { product };

        var warehouseItems = new List<WarehouseItem>
        {
            new() { ProductId = ProductId1, Quantity = 100 }
        };

        _cartRepository.GetCartByUserIdAsync(
                Arg.Is<Guid>(id => id == UserId),
                Arg.Any<CancellationToken>())
            .Returns(cart);

        _warehouseRepository.GetAllWarehouseItemsByProductIdsAsync(
                Arg.Is<List<Guid>>(ids => ids.Contains(ProductId1)),
                Arg.Any<CancellationToken>())
            .Returns(warehouseItems);

        _productRepository.GetProductsByIdsAsync(
                Arg.Is<List<Guid>>(ids => ids.Contains(ProductId1)),
                Arg.Any<CancellationToken>())
            .Returns(products);

        _mapper.Map<OrderModel>(Arg.Any<Order>())
            .Returns(new OrderModel());

        var capturedOrderId = Guid.Empty;
        var capturedUserId = Guid.Empty;

        await _outboxService.StoreEventAsync(
            Arg.Do<OrderCreatedEvent>(e =>
            {
                capturedOrderId = e.OrderId;
                capturedUserId = e.UserId;
            }),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, capturedOrderId);
        Assert.Equal(UserId, capturedUserId);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenSuccessful_ShouldClearCartAfterOrderCreation()
    {
        // Arrange
        var shippingDetails = new ShippingDetailsModel
        {
            FullName = "Test User",
            AddressLine1 = "Test Address",
            City = "Test City",
            Country = "Test Country"
        };
        var command = new CreateOrderCommand(shippingDetails);

        var cartItem = new CartItem { ProductId = ProductId1, Quantity = 1 };
        var cart = new Cart
        {
            Id = CartId,
            UserId = UserId,
            CartItems = new List<CartItem> { cartItem }
        };

        var product = new Product { Id = ProductId1, Price = 100m };
        var products = new List<Product> { product };

        var warehouseItems = new List<WarehouseItem>
        {
            new() { ProductId = ProductId1, Quantity = 100 }
        };

        _cartRepository.GetCartByUserIdAsync(
                Arg.Is<Guid>(id => id == UserId),
                Arg.Any<CancellationToken>())
            .Returns(cart);

        _warehouseRepository.GetAllWarehouseItemsByProductIdsAsync(
                Arg.Is<List<Guid>>(ids => ids.Contains(ProductId1)),
                Arg.Any<CancellationToken>())
            .Returns(warehouseItems);

        _productRepository.GetProductsByIdsAsync(
                Arg.Is<List<Guid>>(ids => ids.Contains(ProductId1)),
                Arg.Any<CancellationToken>())
            .Returns(products);

        _mapper.Map<OrderModel>(Arg.Any<Order>())
            .Returns(new OrderModel());

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        await _cartRepository.Received(1).ClearCartAsync(
            Arg.Is<Guid>(id => id == CartId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenSuccessful_ShouldUseProductPriceAtPurchaseTime()
    {
        // Arrange
        var shippingDetails = new ShippingDetailsModel
        {
            FullName = "Test User",
            AddressLine1 = "Test Address",
            City = "Test City",
            Country = "Test Country"
        };
        var command = new CreateOrderCommand(shippingDetails);

        const decimal currentPrice = 150.00m;

        var cartItem = new CartItem
        {
            ProductId = ProductId1,
            Quantity = 2
        };

        var cart = new Cart
        {
            Id = CartId,
            UserId = UserId,
            CartItems = new List<CartItem> { cartItem }
        };

        var product = new Product { Id = ProductId1, Price = currentPrice };
        var products = new List<Product> { product };

        var warehouseItems = new List<WarehouseItem>
        {
            new() { ProductId = ProductId1, Quantity = 100 }
        };

        _cartRepository.GetCartByUserIdAsync(
                Arg.Is<Guid>(id => id == UserId),
                Arg.Any<CancellationToken>())
            .Returns(cart);

        _warehouseRepository.GetAllWarehouseItemsByProductIdsAsync(
                Arg.Is<List<Guid>>(ids => ids.Contains(ProductId1)),
                Arg.Any<CancellationToken>())
            .Returns(warehouseItems);

        _productRepository.GetProductsByIdsAsync(
                Arg.Is<List<Guid>>(ids => ids.Contains(ProductId1)),
                Arg.Any<CancellationToken>())
            .Returns(products);

        _mapper.Map<OrderModel>(Arg.Any<Order>())
            .Returns(new OrderModel());

        Order capturedOrder = null;
        await _orderRepository.CreateOrderAsync(
            Arg.Do<Order>(o => capturedOrder = o),
            Arg.Any<CancellationToken>());

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedOrder);
        var orderItem = capturedOrder.OrderItems.First();
        Assert.Equal(currentPrice, orderItem.PriceAtPurchase);
        Assert.Equal(currentPrice * 2, capturedOrder.TotalPrice);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenMultipleProducts_ShouldCreateCorrectOrderItems()
    {
        // Arrange
        var shippingDetails = new ShippingDetailsModel
        {
            FullName = "Test User",
            AddressLine1 = "Test Address",
            City = "Test City",
            Country = "Test Country"
        };
        var command = new CreateOrderCommand(shippingDetails);

        var cartItem1 = new CartItem { ProductId = ProductId1, Quantity = 2 };
        var cartItem2 = new CartItem { ProductId = ProductId2, Quantity = 5 };

        var cart = new Cart
        {
            Id = CartId,
            UserId = UserId,
            CartItems = new List<CartItem> { cartItem1, cartItem2 }
        };

        var product1 = new Product { Id = ProductId1, Price = 50m };
        var product2 = new Product { Id = ProductId2, Price = 30m };
        var products = new List<Product> { product1, product2 };

        var warehouseItems = new List<WarehouseItem>
        {
            new() { ProductId = ProductId1, Quantity = 100 },
            new() { ProductId = ProductId2, Quantity = 100 }
        };

        _cartRepository.GetCartByUserIdAsync(
                Arg.Is<Guid>(id => id == UserId),
                Arg.Any<CancellationToken>())
            .Returns(cart);

        _warehouseRepository.GetAllWarehouseItemsByProductIdsAsync(
                Arg.Is<List<Guid>>(ids => ids.Contains(ProductId1) && ids.Contains(ProductId2)),
                Arg.Any<CancellationToken>())
            .Returns(warehouseItems);

        _productRepository.GetProductsByIdsAsync(
                Arg.Is<List<Guid>>(ids => ids.Contains(ProductId1) && ids.Contains(ProductId2)),
                Arg.Any<CancellationToken>())
            .Returns(products);

        _mapper.Map<OrderModel>(Arg.Any<Order>())
            .Returns(new OrderModel());

        Order capturedOrder = null;
        await _orderRepository.CreateOrderAsync(
            Arg.Do<Order>(o => capturedOrder = o),
            Arg.Any<CancellationToken>());

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedOrder);
        Assert.Equal(2, capturedOrder.OrderItems.Count);

        var orderItem1 = capturedOrder.OrderItems.First(oi => oi.ProductId == ProductId1);
        Assert.Equal(2, orderItem1.Quantity);
        Assert.Equal(50m, orderItem1.PriceAtPurchase);

        var orderItem2 = capturedOrder.OrderItems.First(oi => oi.ProductId == ProductId2);
        Assert.Equal(5, orderItem2.Quantity);
        Assert.Equal(30m, orderItem2.PriceAtPurchase);

        Assert.Equal(250m, capturedOrder.TotalPrice);
    }
}