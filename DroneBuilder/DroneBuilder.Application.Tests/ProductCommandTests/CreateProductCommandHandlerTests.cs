using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Mediator.Commands.ProductCommands;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.ProductEvents;
using FluentResults;
using NSubstitute;

namespace DroneBuilder.Application.Tests.ProductCommandTests;

public class CreateProductCommandHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IOutboxEventService _outboxService;
    private readonly CreateProductCommandHandler _handler;

    private const string ProductQueueName = "product-queue";
    private static readonly Guid WarehouseId = Guid.NewGuid();
    private static readonly Guid ProductId = Guid.NewGuid();
    private const string ProductName = "Test Drone";
    private const decimal ProductPrice = 999.99m;
    private const string ProductCategory = "Quadcopter";

    public CreateProductCommandHandlerTests()
    {
        // Arrange
        _productRepository = Substitute.For<IProductRepository>();
        _warehouseRepository = Substitute.For<IWarehouseRepository>();
        _outboxService = Substitute.For<IOutboxEventService>();

        var queuesConfig = new MessageQueuesConfiguration
        {
            ProductQueue = new QueueConfiguration { Name = ProductQueueName }
        };

        _handler = new CreateProductCommandHandler(
            _productRepository,
            _warehouseRepository,
            _outboxService,
            queuesConfig);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenWarehouseExists_ShouldCreateProductSuccessfully()
    {
        // Arrange
        var createProductModel = new CreateProductModel
        {
            Name = ProductName,
            Price = ProductPrice,
            Category = ProductCategory
        };
        var command = new CreateProductCommand(createProductModel);

        var warehouse = new Warehouse { Id = WarehouseId };

        var mappedProduct = new Product
        {
            Id = ProductId,
            Name = ProductName,
            Price = ProductPrice,
            Category = ProductCategory
        };

        var createdProduct = new Product
        {
            Id = ProductId,
            Name = ProductName,
            Price = ProductPrice,
            Category = ProductCategory
        };

        var expectedProductModel = new ProductModel
        {
            Id = ProductId,
            Name = ProductName,
            Price = ProductPrice,
            Category = ProductCategory
        };

        _warehouseRepository.GetWarehouseAsync(Arg.Any<CancellationToken>())
            .Returns(warehouse);

        _productRepository.GetProductByIdAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>())
            .Returns(createdProduct);

        // Act
        Result<ProductModel> result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(ProductName, result.Value.Name);
        Assert.Equal(ProductPrice, result.Value.Price);

        await _productRepository.Received(1).AddProductAsync(
            Arg.Is<Product>(p =>
                p.Name == ProductName &&
                p.Price == ProductPrice),
            Arg.Any<CancellationToken>());

        await _warehouseRepository.Received(1).AddWarehouseItemAsync(
            Arg.Is<WarehouseItem>(wi =>
                wi.WarehouseId == WarehouseId &&
                wi.ProductId != Guid.Empty),
            Arg.Any<CancellationToken>());

        await _outboxService.Received(1).StoreEventAsync(
            Arg.Is<ProductCreatedEvent>(e => e.ProductId != Guid.Empty),
            Arg.Is<string>(q => q == ProductQueueName),
            Arg.Any<CancellationToken>());

        await _productRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        await _productRepository.Received(1).GetProductByIdAsync(
            Arg.Any<Guid>(),
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
        Result<ProductModel> result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<NotFoundError>());

        Assert.Equal("Warehouse not found.", result.Errors[0].Message);

        await _productRepository.DidNotReceive().AddProductAsync(
            Arg.Any<Product>(),
            Arg.Any<CancellationToken>());

        await _warehouseRepository.DidNotReceive().AddWarehouseItemAsync(
            Arg.Any<WarehouseItem>(),
            Arg.Any<CancellationToken>());

        await _outboxService.DidNotReceive().StoreEventAsync(
            Arg.Any<ProductCreatedEvent>(),
            Arg.Any<string>(),
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
            Price = ProductPrice
        };
        var command = new CreateProductCommand(createProductModel);

        var warehouse = new Warehouse { Id = WarehouseId };

        var mappedProduct = new Product
        {
            Id = ProductId,
            Name = ProductName
        };

        _warehouseRepository.GetWarehouseAsync(Arg.Any<CancellationToken>())
            .Returns(warehouse);

        _productRepository.GetProductByIdAsync(Arg.Is<Guid>(id => id == ProductId), Arg.Any<CancellationToken>())
            .Returns(mappedProduct);

        WarehouseItem capturedWarehouseItem = null;
        await _warehouseRepository.AddWarehouseItemAsync(
            Arg.Do<WarehouseItem>(wi => capturedWarehouseItem = wi),
            Arg.Any<CancellationToken>());

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedWarehouseItem);
        Assert.Equal(WarehouseId, capturedWarehouseItem.WarehouseId);
        Assert.NotEqual(Guid.Empty, capturedWarehouseItem.ProductId);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenSuccessful_ShouldGenerateCorrectEvent()
    {
        // Arrange
        var createProductModel = new CreateProductModel
        {
            Name = ProductName,
            Price = ProductPrice
        };
        var command = new CreateProductCommand(createProductModel);

        var warehouse = new Warehouse { Id = WarehouseId };

        var mappedProduct = new Product
        {
            Id = ProductId
        };

        _warehouseRepository.GetWarehouseAsync(Arg.Any<CancellationToken>())
            .Returns(warehouse);

        _productRepository.GetProductByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(mappedProduct);

        Guid capturedProductId = Guid.Empty;
        await _outboxService.StoreEventAsync(
            Arg.Do<ProductCreatedEvent>(e => capturedProductId = e.ProductId),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, capturedProductId);
    }
}

