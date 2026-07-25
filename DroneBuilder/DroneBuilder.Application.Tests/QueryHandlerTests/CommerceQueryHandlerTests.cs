using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Features.Cart.Get;
using DroneBuilder.Application.Features.Cart.GetCartItems;
using DroneBuilder.Application.Features.Orders.GetAdminOrders;
using DroneBuilder.Application.Features.Orders.GetOrders;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Models.CartModels;
using DroneBuilder.Application.Models.OrderModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using NSubstitute;

namespace DroneBuilder.Application.Tests.QueryHandlerTests;

public class CommerceQueryHandlerTests
{
    [Fact]
    public async Task GetCart_UsesAuthenticatedUserAndMapsTotal()
    {
        ICartRepository carts = Substitute.For<ICartRepository>();
        IUserContext user = Substitute.For<IUserContext>();
        user.UserId.Returns(Guid.NewGuid());
        var product = new Product { Id = Guid.NewGuid(), Name = "Motor", Price = 25m };
        var cart = new Cart
        {
            UserId = user.UserId,
            CartItems = [new CartItem { Product = product, ProductId = product.Id, Quantity = 2 }]
        };
        carts.GetCartByUserIdAsync(user.UserId, Arg.Any<CancellationToken>()).Returns(cart);

        Result<CartModel> result = await new GetCartQueryHandler(carts, user)
            .ExecuteAsync(new GetCartByUserIdQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(50m, result.Value.TotalPrice);
        await carts.Received(1).GetCartByUserIdAsync(user.UserId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCart_MissingCart_ReturnsNotFound()
    {
        ICartRepository carts = Substitute.For<ICartRepository>();
        IUserContext user = Substitute.For<IUserContext>();
        user.UserId.Returns(Guid.NewGuid());
        carts.GetCartByUserIdAsync(user.UserId, Arg.Any<CancellationToken>()).Returns((Cart?)null);

        Result<CartModel> result = await new GetCartQueryHandler(carts, user)
            .ExecuteAsync(new GetCartByUserIdQuery(), CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task GetCartItems_ReturnsOnlyCurrentUsersItems()
    {
        ICartRepository carts = Substitute.For<ICartRepository>();
        IUserContext user = Substitute.For<IUserContext>();
        Guid userId = Guid.NewGuid();
        user.UserId.Returns(userId);
        var product = new Product { Id = Guid.NewGuid(), Name = "Frame", Price = 40m };
        carts.GetCartByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new Cart
            {
                UserId = userId,
                CartItems = [new CartItem { Product = product, ProductId = product.Id, Quantity = 1 }]
            });

        Result<ICollection<CartItemModel>> result = await new GetCartItemsQueryHandler(carts, user)
            .ExecuteAsync(new GetCartItemsQuery(), CancellationToken.None);

        Assert.Equal("Frame", Assert.Single(result.Value).ProductName);
    }

    [Fact]
    public async Task GetOrders_UsesAuthenticatedUserAndPreservesPagination()
    {
        IOrderRepository orders = Substitute.For<IOrderRepository>();
        IUserContext user = Substitute.For<IUserContext>();
        Guid userId = Guid.NewGuid();
        user.UserId.Returns(userId);
        var pagination = new PaginationParams(2, 5);
        orders.GetOrdersByUserIdAsync(userId, pagination, Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Order>
            {
                Items = [new Order { Id = Guid.NewGuid(), UserId = userId }],
                TotalCount = 8,
                Page = 2,
                PageSize = 5
            });

        Result<PagedResult<OrderModel>> result = await new GetOrdersQueryHandler(orders, user)
            .ExecuteAsync(new GetOrdersQuery(pagination), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(8, result.Value.TotalCount);
        Assert.Equal(userId, Assert.Single(result.Value.Items).UserId);
    }

    [Fact]
    public async Task GetAdminOrders_ReturnsAllUsersAndPreservesPagination()
    {
        IOrderRepository orders = Substitute.For<IOrderRepository>();
        var pagination = new PaginationParams(1, 20);
        Guid firstUser = Guid.NewGuid();
        Guid secondUser = Guid.NewGuid();
        orders.GetPagedOrdersAsync(pagination, Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Order>
            {
                Items = [new Order { UserId = firstUser }, new Order { UserId = secondUser }],
                TotalCount = 2,
                Page = 1,
                PageSize = 20
            });

        Result<PagedResult<OrderModel>> result = await new GetAdminOrdersQueryHandler(orders)
            .ExecuteAsync(new GetAdminOrdersQuery(pagination), CancellationToken.None);

        Assert.Equal([firstUser, secondUser], result.Value.Items.Select(item => item.UserId));
    }
}
