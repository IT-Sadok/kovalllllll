using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Commands.ProductCommands;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using MapsterMapper;
using NSubstitute;

namespace DroneBuilder.Application.Tests.ProductCommandTests;

public class UpdateProductCommandHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly UpdateProductCommandHandler _handler;

    private static readonly Guid ProductId = Guid.NewGuid();
    private const string OriginalName = "Old Product";
    private const string UpdatedName = "Updated Product";
    private const decimal OriginalPrice = 100m;
    private const decimal UpdatedPrice = 150m;
    private const string OriginalCategory = "Old Category";
    private const string UpdatedCategory = "New Category";

    public UpdateProductCommandHandlerTests()
    {
        // Arrange - створення substitutes
        _productRepository = Substitute.For<IProductRepository>();
        _mapper = Substitute.For<IMapper>();

        _handler = new UpdateProductCommandHandler(
            _productRepository,
            _mapper);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenAllFieldsProvided_ShouldUpdateAllFields()
    {
        // Arrange
        var updateModel = new UpdateProductRequestModel
        {
            Name = UpdatedName,
            Price = UpdatedPrice,
            Category = UpdatedCategory
        };
        var command = new UpdateProductCommand(ProductId, updateModel);

        var existingProduct = new Product
        {
            Id = ProductId,
            Name = OriginalName,
            Price = OriginalPrice,
            Category = OriginalCategory
        };

        var expectedProductModel = new ProductModel
        {
            Id = ProductId,
            Name = UpdatedName,
            Price = UpdatedPrice,
            Category = UpdatedCategory
        };

        _productRepository.GetProductByIdAsync(
                Arg.Is<Guid>(id => id == ProductId),
                Arg.Any<CancellationToken>())
            .Returns(existingProduct);

        _mapper.Map<ProductModel>(Arg.Is<Product>(p =>
                p.Id == ProductId &&
                p.Name == UpdatedName &&
                p.Price == UpdatedPrice &&
                p.Category == UpdatedCategory))
            .Returns(expectedProductModel);

        // Act
        var result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(UpdatedName, result.Name);
        Assert.Equal(UpdatedPrice, result.Price);
        Assert.Equal(UpdatedCategory, result.Category);

        Assert.Equal(UpdatedName, existingProduct.Name);
        Assert.Equal(UpdatedPrice, existingProduct.Price);
        Assert.Equal(UpdatedCategory, existingProduct.Category);

        await _productRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var updateModel = new UpdateProductRequestModel
        {
            Name = UpdatedName
        };
        var command = new UpdateProductCommand(ProductId, updateModel);

        _productRepository.GetProductByIdAsync(
                Arg.Is<Guid>(id => id == ProductId),
                Arg.Any<CancellationToken>())
            .Returns((Product)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal($"Product with id {ProductId} not found.", exception.Message);

        await _productRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());

        _mapper.DidNotReceive().Map<ProductModel>(Arg.Is<Product>(p =>
            p.Name == UpdatedName &&
            p.Price == OriginalPrice &&
            p.Category == OriginalCategory));
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenOnlyNameProvided_ShouldUpdateOnlyName()
    {
        // Arrange
        var updateModel = new UpdateProductRequestModel
        {
            Name = UpdatedName,
            Price = null,
            Category = null
        };
        var command = new UpdateProductCommand(ProductId, updateModel);

        var existingProduct = new Product
        {
            Id = ProductId,
            Name = OriginalName,
            Price = OriginalPrice,
            Category = OriginalCategory
        };

        _productRepository.GetProductByIdAsync(
                Arg.Is<Guid>(id => id == ProductId),
                Arg.Any<CancellationToken>())
            .Returns(existingProduct);

        _mapper.Map<ProductModel>(Arg.Is<Product>(p =>
                p.Name == UpdatedName &&
                p.Price == OriginalPrice &&
                p.Category == OriginalCategory))
            .Returns(new ProductModel());

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(UpdatedName, existingProduct.Name);
        Assert.Equal(OriginalPrice, existingProduct.Price);
        Assert.Equal(OriginalCategory, existingProduct.Category);

        await _productRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenOnlyPriceProvided_ShouldUpdateOnlyPrice()
    {
        // Arrange
        var updateModel = new UpdateProductRequestModel
        {
            Name = null,
            Price = UpdatedPrice,
            Category = null
        };
        var command = new UpdateProductCommand(ProductId, updateModel);

        var existingProduct = new Product
        {
            Id = ProductId,
            Name = OriginalName,
            Price = OriginalPrice,
            Category = OriginalCategory
        };

        _productRepository.GetProductByIdAsync(
                Arg.Is<Guid>(id => id == ProductId),
                Arg.Any<CancellationToken>())
            .Returns(existingProduct);

        _mapper.Map<ProductModel>(Arg.Is<Product>(p =>
                p.Name == OriginalName &&
                p.Price == UpdatedPrice &&
                p.Category == OriginalCategory))
            .Returns(new ProductModel());

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(OriginalName, existingProduct.Name);
        Assert.Equal(UpdatedPrice, existingProduct.Price);
        Assert.Equal(OriginalCategory, existingProduct.Category);

        await _productRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenPartialUpdate_ShouldReturnUpdatedModel()
    {
        // Arrange
        var updateModel = new UpdateProductRequestModel
        {
            Name = UpdatedName,
            Price = UpdatedPrice,
            Category = null
        };
        var command = new UpdateProductCommand(ProductId, updateModel);

        var existingProduct = new Product
        {
            Id = ProductId,
            Name = OriginalName,
            Price = OriginalPrice,
            Category = OriginalCategory
        };

        var expectedProductModel = new ProductModel
        {
            Id = ProductId,
            Name = UpdatedName,
            Price = UpdatedPrice,
            Category = OriginalCategory
        };

        _productRepository.GetProductByIdAsync(
                Arg.Is<Guid>(id => id == ProductId),
                Arg.Any<CancellationToken>())
            .Returns(existingProduct);

        _mapper.Map<ProductModel>(Arg.Is<Product>(p =>
                p.Name == UpdatedName &&
                p.Price == UpdatedPrice &&
                p.Category == OriginalCategory))
            .Returns(expectedProductModel);

        // Act
        var result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(UpdatedName, result.Name);
        Assert.Equal(UpdatedPrice, result.Price);
        Assert.Equal(OriginalCategory, result.Category);
    }
}