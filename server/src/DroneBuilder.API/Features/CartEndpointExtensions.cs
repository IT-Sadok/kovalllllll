using DroneBuilder.API.Common.Extensions;
using DroneBuilder.API.Common.Routes;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Features.Carts;
using DroneBuilder.Application.Features.Carts.AddItemsToCart;
using DroneBuilder.Application.Features.Carts.AddItemToCart;
using DroneBuilder.Application.Features.Carts.ClearCart;
using DroneBuilder.Application.Features.Carts.GetCart;
using DroneBuilder.Application.Features.Carts.GetCartItems;
using DroneBuilder.Application.Features.Carts.RemoveItemFromCart;
using DroneBuilder.Application.Features.Carts.UpdateCartItemQuantity;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace DroneBuilder.API.Features;

public static class CartEndpointExtensions
{
    public static IEndpointRouteBuilder MapCartEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.Cart.AddItemToCart,
                async (IMediator mediator,
                    [FromBody] CreateCartItemModel model,
                    CancellationToken cancellationToken) =>
                {
                    var command = new AddItemToCartCommand(model.ProductId, model.Quantity);

                    Result result = await mediator.ExecuteCommandAsync(command, cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Cart")
            .RequireAuthorization();

        app.MapPost(ApiRoutes.Cart.AddItemsToCart,
                async (IMediator mediator,
                    [FromBody] AddItemsToCartCommand command,
                    CancellationToken cancellationToken) =>
                {
                    Result result = await mediator.ExecuteCommandAsync(command, cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Cart")
            .RequireAuthorization();

        app.MapDelete(ApiRoutes.Cart.ClearCart,
                async (IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var command = new ClearCartCommand();

                    Result result = await mediator.ExecuteCommandAsync(command, cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Cart")
            .RequireAuthorization();

        app.MapDelete(ApiRoutes.Cart.RemoveItemFromCart,
                async (IMediator mediator, Guid productId,
                    CancellationToken cancellationToken) =>
                {
                    var command = new RemoveItemFromCartCommand(productId);

                    Result result = await mediator.ExecuteCommandAsync(command, cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Cart")
            .RequireAuthorization();

        app.MapPatch(ApiRoutes.Cart.UpdateItemQuantity,
                async (IMediator mediator, Guid productId, [FromBody] int quantity,
                    CancellationToken cancellationToken) =>
                {
                    var command = new UpdateCartItemQuantityCommand(productId, quantity);

                    Result result = await mediator.ExecuteCommandAsync(command, cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Cart")
            .RequireAuthorization();

        app.MapGet(ApiRoutes.Cart.GetCartItems,
                async (IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var query = new GetCartItemsQuery();
                    Result<ICollection<CartItemModel>> result = await mediator.ExecuteQueryAsync<GetCartItemsQuery, ICollection<CartItemModel>>(
                        query,
                        cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Cart")
            .RequireAuthorization();

        app.MapGet(ApiRoutes.Cart.GetCart,
                async (IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var query = new GetCartQuery();
                    Result<CartModel> result = await mediator.ExecuteQueryAsync<GetCartQuery, CartModel>(query,
                        cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Cart")
            .RequireAuthorization();

        return app;
    }
}
