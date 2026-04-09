using DroneBuilder.API.Authorization;
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
                async (IMediator mediator, CreateProductModel model, CancellationToken cancellationToken) =>
                {
                    var result = await mediator.ExecuteCommandAsync<CreateProductCommand, ProductModel>(
                        new CreateProductCommand(model),
                        cancellationToken);
                    return Results.Ok(result);
                }).WithTags("Products")
            .RequireAuthorization();

        app.MapPatch(ApiRoutes.Products.Update,
                async (Guid productId, UpdateProductRequestModel requestModel,
                    IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var result = await mediator.ExecuteCommandAsync<UpdateProductCommand, ProductModel>(
                        new UpdateProductCommand(productId, requestModel),
                        cancellationToken);
                    return Results.Ok(result);
                }).WithTags("Products")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapDelete(ApiRoutes.Products.Delete, async (IMediator mediator, Guid productId,
                CancellationToken cancellationToken) =>
            {
                await mediator.ExecuteCommandAsync(new DeleteProductCommand(productId), cancellationToken);
                return Results.NoContent();
            }).WithTags("Products")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapGet(ApiRoutes.Products.GetAll,
                async (int page, int pageSize, IMediator mediator, [AsParameters] ProductFilterModel filter,
                    CancellationToken cancellationToken) =>
                {
                    var pagination = new PaginationParams(page, pageSize);
                    var query = new GetProductsQuery(pagination, filter);
                    var result = await mediator.ExecuteQueryAsync<GetProductsQuery, PagedResult<ProductModel>>(
                        query,
                        cancellationToken);
                    return Results.Ok(result);
                }).WithTags("Products");

        app.MapGet(ApiRoutes.Products.GetCategories,
                async (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var result = await mediator.ExecuteQueryAsync<GetCategoriesQuery, IEnumerable<string>>(
                        new GetCategoriesQuery(),
                        cancellationToken);
                    return Results.Ok(result);
                }).WithTags("Products");

        app.MapGet(ApiRoutes.Products.GetById,
                async (IMediator mediator, Guid productId, CancellationToken cancellationToken) =>
                {
                    var result = await mediator.ExecuteQueryAsync<GetProductByIdQuery, ProductModel>(
                        new GetProductByIdQuery(productId),
                        cancellationToken);
                    return Results.Ok(result);
                }).WithTags("Products");


        app.MapGet(ApiRoutes.Products.GetPropertiesByProductId,
                async (IMediator mediator, Guid productId, CancellationToken cancellationToken) =>
                {
                    var result =
                        await mediator.ExecuteQueryAsync<GetPropertiesByProductIdQuery, ProductPropertiesResponseModel>(
                            new GetPropertiesByProductIdQuery(productId),
                            cancellationToken);
                    return Results.Ok(result);
                }).WithTags("Products");

        app.MapPost(ApiRoutes.Products.AssignValueToProductProperty, async (IMediator mediator, Guid productId, Guid propertyId, Guid valueId,
                CancellationToken cancellationToken) =>
            {
                await mediator.ExecuteCommandAsync(new AddValueToProductPropertyCommand(productId, propertyId, valueId),
                    cancellationToken);
                return Results.NoContent();
            }).WithTags("Products")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapDelete(ApiRoutes.Products.RemoveValueFromProductProperty, async (IMediator mediator, Guid productId, Guid propertyId, Guid valueId,
                CancellationToken cancellationToken) =>
            {
                await mediator.ExecuteCommandAsync(new RemoveValueFromProductPropertyCommand(productId, propertyId, valueId),
                    cancellationToken);
                return Results.NoContent();
            }).WithTags("Products")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapDelete(ApiRoutes.Products.RemovePropertyFromProduct, async (IMediator mediator, Guid productId, Guid propertyId,
                CancellationToken cancellationToken) =>
            {
                await mediator.ExecuteCommandAsync(new RemovePropertyFromProductCommand(productId, propertyId),
                    cancellationToken);
                return Results.NoContent();
            }).WithTags("Products")
            .RequireAuthorization(PolicyNames.Admin);

        return app;
    }
}