using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Commands.ProductCommands;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.ProductEvents;
using MapsterMapper;
using NSubstitute;

namespace DroneBuilder.Application.Tests.ProductCommandTests;

public class CreateProductCommandHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IOutboxEventService _outboxService;
    private readonly IMapper _mapper;
    private readonly CreateProductCommandHandler _handler;

    private const string ProductQueueName = "product-queue";
    private static readonly Guid WarehouseId = Guid.NewGuid();
    private static readonly Guid ProductId = Guid.NewGuid();
    private const string ProductName = "Test Drone";
    private const string PropertyName = "Color";
    private const decimal ProductPrice = 999.99m;

    public CreateProductCommandHandlerTests()
    {
        // Arrange
        _productRepository = Substitute.For<IProductRepository>();
        _warehouseRepository = Substitute.For<IWarehouseRepository>();
        _outboxService = Substitute.For<IOutboxEventService>();
        _mapper = Substitute.For<IMapper>();

        var queuesConfig = new MessageQueuesConfiguration
        {
            ProductQueue = new QueueConfiguration { Name = ProductQueueName }
        };

        _handler = new CreateProductCommandHandler(
            _productRepository,
            _warehouseRepository,
            _outboxService,
            queuesConfig,
            _mapper);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenWarehouseExists_ShouldCreateProductSuccessfully()
    {
        // Arrange
        var createProductModel = new CreateProductModel
        {
            Name = ProductName,
            Price = ProductPrice,
            Properties = new CreatePropertyModel()
        };
        var command = new CreateProductCommand(createProductModel);

        var warehouse = new Warehouse { Id = WarehouseId };

        var mappedProduct = new Product
        {
            Id = ProductId,
            Name = ProductName,
            Price = ProductPrice,
            Properties = new List<Property>()
        };

        var mappedProperty = new Property { Id = Guid.NewGuid() };

        var createdProduct = new Product
        {
            Id = ProductId,
            Name = ProductName,
            Price = ProductPrice
        };

        var expectedProductModel = new ProductModel
        {
            Id = ProductId,
            Name = ProductName,
            Price = ProductPrice
        };

        _warehouseRepository.GetWarehouseAsync(Arg.Any<CancellationToken>())
            .Returns(warehouse);

        _mapper.Map<Product>(Arg.Is<CreateProductModel>(m =>
                m.Name == ProductName &&
                m.Price == ProductPrice))
            .Returns(mappedProduct);

        _mapper.Map<Property>(Arg.Is<CreatePropertyModel>(p => p.Name == PropertyName))
            .Returns(mappedProperty);

        _productRepository.GetProductByIdAsync(
                Arg.Is<Guid>(id => id == ProductId),
                Arg.Any<CancellationToken>())
            .Returns(createdProduct);

        _mapper.Map<ProductModel>(Arg.Is<Product>(p => p.Id == ProductId))
            .Returns(expectedProductModel);

        // Act
        var result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ProductId, result.Id);
        Assert.Equal(ProductName, result.Name);
        Assert.Equal(ProductPrice, result.Price);

        await _productRepository.Received(1).AddProductAsync(
            Arg.Is<Product>(p =>
                p.Name == ProductName &&
                p.Price == ProductPrice),
            Arg.Any<CancellationToken>());

        await _warehouseRepository.Received(1).AddWarehouseItemAsync(
            Arg.Is<WarehouseItem>(wi =>
                wi.WarehouseId == WarehouseId &&
                wi.ProductId == ProductId),
            Arg.Any<CancellationToken>());

        await _outboxService.Received(1).StoreEventAsync(
            Arg.Is<ProductCreatedEvent>(e => e.ProductId == ProductId),
            Arg.Is<string>(q => q == ProductQueueName),
            Arg.Any<CancellationToken>());

        await _productRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        await _productRepository.Received(1).GetProductByIdAsync(
            Arg.Is<Guid>(id => id == ProductId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenWarehouseNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var createProductModel = new CreateProductModel
        {
            Name = ProductName,
            Price = ProductPrice
        };
        var command = new CreateProductCommand(createProductModel);

        _warehouseRepository.GetWarehouseAsync(Arg.Any<CancellationToken>())
            .Returns((Warehouse)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal("Warehouse not found.", exception.Message);

        await _productRepository.DidNotReceive().AddProductAsync(
            Arg.Is<Product>(p =>
                p.Name == ProductName &&
                p.Price == ProductPrice),
            Arg.Any<CancellationToken>());

        await _warehouseRepository.DidNotReceive().AddWarehouseItemAsync(
            Arg.Is<WarehouseItem>(wi =>
                wi.WarehouseId == WarehouseId &&
                wi.ProductId == ProductId),
            Arg.Any<CancellationToken>());

        await _outboxService.DidNotReceive().StoreEventAsync(
            Arg.Is<ProductCreatedEvent>(e => e.ProductId == ProductId),
            Arg.Is<string>(q => q == ProductQueueName),
            Arg.Any<CancellationToken>());

        await _productRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenSuccessful_ShouldCreateWarehouseItemWithCorrectIds()
    {
        // Arrange
        var createProductModel = new CreateProductModel
        {
            Name = ProductName,
            Price = ProductPrice,
            Properties = new CreatePropertyModel()
        };
        var command = new CreateProductCommand(createProductModel);

        var warehouse = new Warehouse { Id = WarehouseId };

        var mappedProduct = new Product
        {
            Id = ProductId,
            Name = ProductName,
            Properties = new List<Property>()
        };

        _warehouseRepository.GetWarehouseAsync(Arg.Any<CancellationToken>())
            .Returns(warehouse);

        _mapper.Map<Product>(Arg.Is<CreateProductModel>(m =>
                m.Name == ProductName &&
                m.Price == ProductPrice))
            .Returns(mappedProduct);

        _mapper.Map<Property>(Arg.Is<CreatePropertyModel>(p => p.Name == PropertyName))
            .Returns(new Property());

        _productRepository.GetProductByIdAsync(Arg.Is<Guid>(id => id == ProductId), Arg.Any<CancellationToken>())
            .Returns(mappedProduct);

        _mapper.Map<ProductModel>(Arg.Is<Product>(p => p.Id == ProductId))
            .Returns(new ProductModel());

        WarehouseItem capturedWarehouseItem = null;
        await _warehouseRepository.AddWarehouseItemAsync(
            Arg.Do<WarehouseItem>(wi => capturedWarehouseItem = wi),
            Arg.Any<CancellationToken>());

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedWarehouseItem);
        Assert.Equal(WarehouseId, capturedWarehouseItem.WarehouseId);
        Assert.Equal(ProductId, capturedWarehouseItem.ProductId);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenSuccessful_ShouldAddPropertyToProduct()
    {
        // Arrange
        var createProductModel = new CreateProductModel
        {
            Name = ProductName,
            Price = ProductPrice,
            Properties = new CreatePropertyModel()
        };
        var command = new CreateProductCommand(createProductModel);

        var warehouse = new Warehouse { Id = WarehouseId };

        var mappedProduct = new Product
        {
            Id = ProductId,
            Name = ProductName,
            Properties = new List<Property>()
        };

        var mappedProperty = new Property { Id = Guid.NewGuid() };

        _warehouseRepository.GetWarehouseAsync(Arg.Any<CancellationToken>())
            .Returns(warehouse);

        _mapper.Map<Product>(Arg.Is<CreateProductModel>(m =>
                m.Name == ProductName &&
                m.Price == ProductPrice))
            .Returns(mappedProduct);

        _mapper.Map<Property>(Arg.Any<CreatePropertyModel>())
            .Returns(mappedProperty);

        _productRepository.GetProductByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(mappedProduct);

        _mapper.Map<ProductModel>(Arg.Any<Product>())
            .Returns(new ProductModel());

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(mappedProduct.Properties);
        Assert.Contains(mappedProperty, mappedProduct.Properties);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenSuccessful_ShouldGenerateCorrectEvent()
    {
        // Arrange
        var createProductModel = new CreateProductModel
        {
            Name = ProductName,
            Price = ProductPrice,
            Properties = new CreatePropertyModel()
        };
        var command = new CreateProductCommand(createProductModel);

        var warehouse = new Warehouse { Id = WarehouseId };

        var mappedProduct = new Product
        {
            Id = ProductId,
            Properties = new List<Property>()
        };

        _warehouseRepository.GetWarehouseAsync(Arg.Any<CancellationToken>())
            .Returns(warehouse);

        _mapper.Map<Product>(Arg.Any<CreateProductModel>())
            .Returns(mappedProduct);

        _mapper.Map<Property>(Arg.Any<CreatePropertyModel>())
            .Returns(new Property());

        _productRepository.GetProductByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(mappedProduct);

        _mapper.Map<ProductModel>(Arg.Any<Product>())
            .Returns(new ProductModel());

        var capturedProductId = Guid.Empty;
        await _outboxService.StoreEventAsync(
            Arg.Do<ProductCreatedEvent>(e => capturedProductId = e.ProductId),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(ProductId, capturedProductId);
    }
}