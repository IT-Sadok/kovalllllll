using DroneBuilder.Application.Features.Catalog.Products.UpdateProduct;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using NSubstitute;

namespace DroneBuilder.Application.Tests.ProductCommandTests;

public class UpdateProductCommandHandlerTests
{
    private readonly IProductRepository _productRepository;
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

        _handler = new UpdateProductCommandHandler(
            _productRepository);
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

        // Act
        Result<ProductModel> result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(UpdatedName, result.Value.Name);
        Assert.Equal(UpdatedPrice, result.Value.Price);
        Assert.Equal(UpdatedCategory, result.Value.Category);

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
            .Returns((Product?)null);

        // Act & Assert
        Result<ProductModel> result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<NotFoundError>());

        Assert.Equal($"Product with id {ProductId} not found.", result.Errors[0].Message);

        await _productRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());

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

        // Act
        Result<ProductModel> result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(UpdatedName, result.Value.Name);
        Assert.Equal(UpdatedPrice, result.Value.Price);
        Assert.Equal(OriginalCategory, result.Value.Category);
    }
}

