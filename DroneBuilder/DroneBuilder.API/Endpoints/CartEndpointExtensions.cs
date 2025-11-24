using DroneBuilder.API.Endpoints.Routes;
using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Mediator.Commands.CartCommands;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Mediator.Queries.CartQueries;
using DroneBuilder.Application.Models.CartModels;
using Microsoft.AspNetCore.Mvc;

namespace DroneBuilder.API.Endpoints;

public static class CartEndpointExtensions
{
    public static IEndpointRouteBuilder MapCartEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.Cart.AddItemToCart,
                async ([FromServices] IUserContext userContext, IMediator mediator, [FromBody] CreateCartItemModel model,
                    CancellationToken cancellationToken) =>
                {
                    var command = new AddItemToCartCommand(userContext.UserId, model.ProductId, model.Quantity);

                    await mediator.ExecuteCommandAsync(command, cancellationToken);
                    return Results.Ok();
                })
            .WithTags("Cart")
            .RequireAuthorization();

        app.MapDelete(ApiRoutes.Cart.ClearCart,
                async ([FromServices] IUserContext userContext, IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var command = new ClearCartCommand(userContext.UserId);

                    await mediator.ExecuteCommandAsync(command, cancellationToken);
                    return Results.NoContent();
                })
            .WithTags("Cart")
            .RequireAuthorization();
        app.MapDelete(ApiRoutes.Cart.RemoveItemFromCart,
                async ([FromServices] IUserContext userContext, IMediator mediator, Guid productId,
                    CancellationToken cancellationToken) =>
                {
                    var command = new RemoveItemFromCartCommand(userContext.UserId, productId);

                    await mediator.ExecuteCommandAsync(command, cancellationToken);
                    return Results.NoContent();
                })
            .WithTags("Cart")
            .RequireAuthorization();


        app.MapGet(ApiRoutes.Cart.GetCartItems,
                async ([FromServices] IUserContext userContext, IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var query = new GetCartItemsQuery(userContext.UserId);
                    var cartItems = await mediator.ExecuteQueryAsync<GetCartItemsQuery, ICollection<CartItemModel>>(
                        query,
                        cancellationToken);
                    return Results.Ok(cartItems);
                })
            .WithTags("Cart")
            .RequireAuthorization();

        app.MapGet(ApiRoutes.Cart.GetCart,
                async ([FromServices] IUserContext userContext, IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var query = new GetCartByUserIdQuery(userContext.UserId);
                    var cart = await mediator.ExecuteQueryAsync<GetCartByUserIdQuery, CartModel>(query,
                        cancellationToken);
                    return Results.Ok(cart);
                })
            .WithTags("Cart")
            .RequireAuthorization();

        return app;
    }
}