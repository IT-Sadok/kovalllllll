using System.Security.Claims;
using DroneBuilder.API.Endpoints.Routes;
using DroneBuilder.Application.Mediator.Commands.CartCommands;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Mediator.Queries.CartQueries;
using DroneBuilder.Application.Models.CartModels;

namespace DroneBuilder.API.Endpoints;

public static class CartEndpointExtensions
{
    public static IEndpointRouteBuilder MapCartEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.Cart.AddItemToCart,
                async (ClaimsPrincipal user, IMediator mediator, CreateCartItemModel model,
                    CancellationToken cancellationToken) =>
                {
                    var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!Guid.TryParse(userIdString, out var userId))
                    {
                        return Results.Unauthorized();
                    }

                    var command = new AddItemToCartCommand(userId, model.ProductId, model.Quantity);

                    await mediator.ExecuteCommandAsync(command, cancellationToken);
                    return Results.Ok();
                })
            .WithTags("Cart")
            .RequireAuthorization();

        app.MapDelete(ApiRoutes.Cart.ClearCart,
                async (ClaimsPrincipal user, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!Guid.TryParse(userIdString, out var userId))
                    {
                        return Results.Unauthorized();
                    }

                    var command = new ClearCartCommand(userId);

                    await mediator.ExecuteCommandAsync(command, cancellationToken);
                    return Results.NoContent();
                })
            .WithTags("Cart")
            .RequireAuthorization();

        app.MapDelete(ApiRoutes.Cart.RemoveItemFromCart,
                async (ClaimsPrincipal user, IMediator mediator, Guid productId, CancellationToken cancellationToken) =>
                {
                    var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!Guid.TryParse(userIdString, out var userId))
                    {
                        return Results.Unauthorized();
                    }

                    var command = new RemoveItemFromCartCommand(userId, productId);

                    await mediator.ExecuteCommandAsync(command, cancellationToken);
                    return Results.NoContent();
                })
            .WithTags("Cart")
            .RequireAuthorization();

        app.MapGet(ApiRoutes.Cart.GetCartItems,
                async (ClaimsPrincipal user, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!Guid.TryParse(userIdString, out var userId))
                    {
                        return Results.Unauthorized();
                    }

                    var query = new GetCartItemsQuery(userId);
                    var cartItems = await mediator.ExecuteQueryAsync<GetCartItemsQuery, ICollection<CartItemModel>>(
                        query,
                        cancellationToken);
                    return Results.Ok(cartItems);
                })
            .WithTags("Cart")
            .RequireAuthorization();

        app.MapGet(ApiRoutes.Cart.GetCartByUserId,
                async (ClaimsPrincipal user, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!Guid.TryParse(userIdString, out var userId))
                    {
                        return Results.Unauthorized();
                    }

                    var query = new GetCartByUserIdQuery(userId);
                    var cart = await mediator.ExecuteQueryAsync<GetCartByUserIdQuery, CartModel>(query,
                        cancellationToken);
                    return Results.Ok(cart);
                })
            .WithTags("Cart")
            .RequireAuthorization();

        return app;
    }
}