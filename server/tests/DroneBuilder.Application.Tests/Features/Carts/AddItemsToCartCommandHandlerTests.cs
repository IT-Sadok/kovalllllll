using DroneBuilder.Application.Common.Abstractions;
using DroneBuilder.Application.Common.Contexts;
using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Options;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Features.Builds.CheckBuild;
using DroneBuilder.Application.Features.Carts.AddItemsToCart;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.CartEvents;
using FluentResults;
using FluentValidation.Results;
using NSubstitute;

namespace DroneBuilder.Application.Tests.Features.Carts;

public class AddItemsToCartCommandHandlerTests
{
    private const string CartQueueName = "cart-queue";
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly ICartRepository _cartRepository = Substitute.For<ICartRepository>();
    private readonly IOutboxEventService _outboxService = Substitute.For<IOutboxEventService>();
    private readonly IWarehouseRepository _warehouseRepository = Substitute.For<IWarehouseRepository>();
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly AddItemsToCartCommandHandler _handler;

    private readonly Product _frame = new() { Name = "Frame" };
    private readonly Product _motor = new() { Name = "Motor" };
    private readonly WarehouseItem _frameStock;
    private readonly WarehouseItem _motorStock;

    public AddItemsToCartCommandHandlerTests()
    {
        IUserContext userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(UserId);

        _frameStock = new WarehouseItem { ProductId = _frame.Id, Quantity = 10 };
        _motorStock = new WarehouseItem { ProductId = _motor.Id, Quantity = 10 };

        _productRepository.GetProductsByIdsAsync(Arg.Any<ICollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(call => new[] { _frame, _motor }.Where(p => call.Arg<ICollection<Guid>>().Contains(p.Id)).ToList());
        _warehouseRepository
            .GetTrackedWarehouseItemsByProductIdsAsync(Arg.Any<ICollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(call => new[] { _frameStock, _motorStock }
                .Where(w => call.Arg<ICollection<Guid>>().Contains(w.ProductId)).ToList());

        _handler = new AddItemsToCartCommandHandler(
            _cartRepository,
            _outboxService,
            _warehouseRepository,
            _productRepository,
            new MessageQueuesConfiguration { CartQueue = new QueueConfiguration { Name = CartQueueName } },
            userContext);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenStockIsEnough_ShouldReserveEveryPartInOneSave()
    {
        // Arrange
        var existingFrame = new CartItem { ProductId = _frame.Id, ProductName = _frame.Name, Quantity = 1 };
        var cart = new Cart { UserId = UserId, CartItems = [existingFrame] };
        _cartRepository.GetCartByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(cart);

        // Act
        Result result = await _handler.ExecuteCommandAsync(
            new AddItemsToCartCommand([new BuildItemModel(_frame.Id, 1), new BuildItemModel(_motor.Id, 4)]),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, existingFrame.Quantity);
        await _cartRepository.Received(1).AddCartItemAsync(
            Arg.Is<CartItem>(ci => ci.ProductId == _motor.Id && ci.Quantity == 4 && ci.Cart == cart),
            Arg.Any<CancellationToken>());
        Assert.Equal(9, _frameStock.Quantity);
        Assert.Equal(6, _motorStock.Quantity);
        await _outboxService.Received(2).StoreEventAsync(Arg.Any<AddedItemToCartEvent>(), CartQueueName,
            Arg.Any<CancellationToken>());
        await _cartRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenUserHasNoCart_ShouldCreateOne()
    {
        // Arrange
        _cartRepository.GetCartByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns((Cart?)null);

        // Act
        Result result = await _handler.ExecuteCommandAsync(
            new AddItemsToCartCommand([new BuildItemModel(_frame.Id, 1)]), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await _cartRepository.Received(1).CreateCartAsync(Arg.Is<Cart>(c => c.UserId == UserId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenAnyPartIsShort_ShouldReserveNothingAndNameEachShortPart()
    {
        // Arrange
        _motorStock.Quantity = 3;

        // Act
        Result result = await _handler.ExecuteCommandAsync(
            new AddItemsToCartCommand([new BuildItemModel(_frame.Id, 1), new BuildItemModel(_motor.Id, 4)]),
            CancellationToken.None);

        // Assert
        Assert.True(result.HasError<BadRequestError>());
        Assert.Contains("Motor (available 3, requested 4)", result.Errors[0].Message);
        Assert.Equal(10, _frameStock.Quantity);
        await _cartRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenProductIsUnknown_ShouldReturnNotFound()
    {
        // Act
        Result result = await _handler.ExecuteCommandAsync(
            new AddItemsToCartCommand([new BuildItemModel(Guid.NewGuid(), 1)]), CancellationToken.None);

        // Assert
        Assert.True(result.HasError<NotFoundError>());
        await _cartRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Validator_WhenItemsAreEmpty_ShouldFail()
    {
        // Act
        ValidationResult result = new AddItemsToCartCommandValidator().Validate(new AddItemsToCartCommand([]));

        // Assert
        Assert.False(result.IsValid);
    }
}
