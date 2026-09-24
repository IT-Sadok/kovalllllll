using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Features.Products;
using DroneBuilder.Application.Features.Products.SetProductAttributes;
using DroneBuilder.Domain.Entities;
using FluentResults;
using NSubstitute;

namespace DroneBuilder.Application.Tests.Features.Products;

public class SetProductAttributesCommandHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly SetProductAttributesCommandHandler _handler;

    private static readonly Guid ProductId = Guid.NewGuid();

    public SetProductAttributesCommandHandlerTests()
    {
        // Arrange
        _productRepository = Substitute.For<IProductRepository>();

        _handler = new SetProductAttributesCommandHandler(_productRepository);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenProductExists_ShouldReplaceAttributesInGivenOrder()
    {
        // Arrange
        var product = new Product
        {
            Id = ProductId,
            Attributes = [new ProductAttribute { ProductId = ProductId, Name = "Color", Value = "Red" }]
        };

        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(product);

        var command = new SetProductAttributesCommand(ProductId,
        [
            new ProductAttributeModel(" Wheelbase ", " 226 mm "),
            new ProductAttributeModel("Material", "Carbon T700")
        ]);

        // Act
        Result<ProductModel> result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Collection(product.Attributes.OrderBy(a => a.SortOrder),
            a =>
            {
                Assert.Equal("Wheelbase", a.Name);
                Assert.Equal("226 mm", a.Value);
                Assert.Equal(0, a.SortOrder);
            },
            a =>
            {
                Assert.Equal("Material", a.Name);
                Assert.Equal(1, a.SortOrder);
            });
        Assert.Equal(
            [new ProductAttributeModel("Wheelbase", "226 mm"), new ProductAttributeModel("Material", "Carbon T700")],
            result.Value.Attributes);
        await _productRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenListIsEmpty_ShouldRemoveAllAttributes()
    {
        // Arrange
        var product = new Product
        {
            Id = ProductId,
            Attributes = [new ProductAttribute { ProductId = ProductId, Name = "Color", Value = "Red" }]
        };

        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(product);

        // Act
        Result<ProductModel> result =
            await _handler.ExecuteCommandAsync(new SetProductAttributesCommand(ProductId, []), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(product.Attributes);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenProductNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        // Act
        Result<ProductModel> result =
            await _handler.ExecuteCommandAsync(new SetProductAttributesCommand(ProductId, []), CancellationToken.None);

        // Assert
        Assert.True(result.HasError<NotFoundError>());
        await _productRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
