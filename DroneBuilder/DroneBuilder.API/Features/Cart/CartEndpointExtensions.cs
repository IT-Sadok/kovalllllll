using DroneBuilder.API.Endpoints.Routes;
using DroneBuilder.API.Extensions;
using DroneBuilder.Application.Features.Cart.AddItemToCart;
using DroneBuilder.Application.Features.Cart.ClearCart;
using DroneBuilder.Application.Features.Cart.Get;
using DroneBuilder.Application.Features.Cart.GetCartItems;
using DroneBuilder.Application.Features.Cart.RemoveItemFromCart;
using DroneBuilder.Application.Features.Cart.UpdateCartItemQuantity;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.CartModels;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace DroneBuilder.API.Features.Cart;

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
                async (IMediator mediator, Guid itemId,
                    CancellationToken cancellationToken) =>
                {
                    // Retained route shape; this value has always represented a product ID.
                    var command = new RemoveItemFromCartCommand(itemId);

                    Result result = await mediator.ExecuteCommandAsync(command, cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Cart")
            .RequireAuthorization();

        app.MapPatch(ApiRoutes.Cart.AddItemToCart + "/{productId}",
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
                    var query = new GetCartByUserIdQuery();
                    Result<CartModel> result = await mediator.ExecuteQueryAsync<GetCartByUserIdQuery, CartModel>(query,
                        cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Cart")
            .RequireAuthorization();

        return app;
    }
}
