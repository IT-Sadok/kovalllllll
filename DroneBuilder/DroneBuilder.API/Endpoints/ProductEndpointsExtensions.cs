using DroneBuilder.API.Endpoints.Routes;
using DroneBuilder.Application.Mediator.Commands.ProductCommands;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Mediator.Queries.ProductQueries;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Models.ProductModels;

namespace DroneBuilder.API.Endpoints;

public static class ProductEndpointsExtensions
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.Products.Create,
            async (IMediator mediator, CreateProductRequestModel requestModel, CancellationToken cancellationToken) =>
            {
                var result = await mediator.ExecuteCommandAsync<CreateProductCommand, ProductResponseModel>(
                    new CreateProductCommand(requestModel),
                    cancellationToken);
                return Results.Ok(result);
            });

        app.MapPatch(ApiRoutes.Products.Update, async (IMediator mediator, UpdateProductRequestModel requestModel,
            Guid productId, CancellationToken cancellationToken) =>
        {
            var result = await mediator.ExecuteCommandAsync<UpdateProductCommand, ProductResponseModel>(
                new UpdateProductCommand(productId, requestModel),
                cancellationToken);
            return Results.Ok(result);
        });

        app.MapDelete(ApiRoutes.Products.Delete, async (IMediator mediator, Guid productId,
            CancellationToken cancellationToken) =>
        {
            await mediator.ExecuteCommandAsync(new DeleteProductCommand(new Domain.Entities.Product { Id = productId }),
                cancellationToken);
            return Results.NoContent();
        });

        app.MapGet(ApiRoutes.Products.GetAll,
            async (IMediator mediator, CancellationToken cancellationToken) =>
            {
                var result = await mediator.ExecuteQueryAsync<GetProductsQuery, ProductsResponseModel>(
                    new GetProductsQuery(),
                    cancellationToken);
                return Results.Ok(result);
            });

        app.MapGet(ApiRoutes.Products.GetById,
            async (IMediator mediator, Guid productId, CancellationToken cancellationToken) =>
            {
                var result = await mediator.ExecuteQueryAsync<GetProductByIdQuery, ProductResponseModel>(
                    new GetProductByIdQuery(productId),
                    cancellationToken);
                return Results.Ok(result);
            });


        app.MapGet(ApiRoutes.Products.GetPropertiesByProductId,
            async (IMediator mediator, Guid productId, CancellationToken cancellationToken) =>
            {
                var result =
                    await mediator.ExecuteQueryAsync<GetPropertiesByProductIdQuery, ProductPropertiesResponseModel>(
                        new GetPropertiesByProductIdQuery(productId),
                        cancellationToken);
                return Results.Ok(result);
            });

        return app;
    }
}