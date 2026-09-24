using DroneBuilder.Application.Common.Abstractions;
using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Features.Products;
using DroneBuilder.Application.Features.Products.SetProductSpec;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Entities.Components;
using FluentResults;
using NSubstitute;

namespace DroneBuilder.Application.Tests.Features.Products;

public class SetProductSpecCommandHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly ITransaction _transaction;
    private readonly SetProductSpecCommandHandler _handler;

    private static readonly Guid ProductId = Guid.NewGuid();

    private static readonly MotorSpecModel MotorSpec =
        new("2207", 1950, MountPattern.M16x16, 6, 6, 45m, 5m);

    public SetProductSpecCommandHandlerTests()
    {
        // Arrange
        _productRepository = Substitute.For<IProductRepository>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        _transaction = Substitute.For<ITransaction>();
        unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(_transaction);

        _handler = new SetProductSpecCommandHandler(_productRepository, unitOfWork);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenProductExists_ShouldSetSpecAndSave()
    {
        // Arrange
        var product = new Product { Id = ProductId, Name = "Motor", Price = 25m, Category = "Motors" };

        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(product);

        // Act
        Result<ProductModel> result =
            await _handler.ExecuteCommandAsync(new SetProductSpecCommand(ProductId, MotorSpec), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        MotorSpec motor = Assert.IsType<MotorSpec>(product.Spec);
        Assert.Equal(ProductId, motor.ProductId);
        Assert.Equal(ComponentType.Motor, motor.Type);
        Assert.Equal(1950, motor.Kv);
        Assert.Equal(MotorSpec, result.Value.Spec);
        await _productRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenProductHasSpec_ShouldDeleteOldSpecBeforeAddingNew()
    {
        // Arrange
        var product = new Product
        {
            Id = ProductId,
            Spec = new BatterySpec { ProductId = ProductId, Cells = 6 }
        };

        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns(product);

        // Act
        Result<ProductModel> result =
            await _handler.ExecuteCommandAsync(new SetProductSpecCommand(ProductId, MotorSpec), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.IsType<MotorSpec>(product.Spec);
        await _productRepository.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenProductNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _productRepository.GetProductByIdAsync(ProductId, Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        // Act
        Result<ProductModel> result =
            await _handler.ExecuteCommandAsync(new SetProductSpecCommand(ProductId, MotorSpec), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.True(result.HasError<NotFoundError>());
        await _productRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _transaction.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }
}
