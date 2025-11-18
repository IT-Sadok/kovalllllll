using DroneBuilder.API.Endpoints.Routes;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Mediator.Queries.Filters;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;

namespace DroneBuilder.API.Endpoints;

public static class FilterEndpointExtensions
{
    public static IEndpointRouteBuilder MapFilterEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRoutes.Filters.GetProductsByCategory,
            async (IMediator mediator, string categoryName, CancellationToken cancellationToken) =>
            {
                var query = new GetProductsByCategoryQuery(categoryName);

                var result =
                    await mediator.ExecuteQueryAsync<GetProductsByCategoryQuery, ProductsResponseModel>(query,
                        cancellationToken);
                return Results.Ok(result);
            }).WithTags("Filters");

        app.MapGet(ApiRoutes.Filters.GetProductsByPrice, async (IMediator mediator, decimal? minPrice,
            decimal? maxPrice,
            CancellationToken cancellationToken) =>
        {
            var query = new GetProductsByPriceQuery(minPrice, maxPrice);

            var result =
                await mediator.ExecuteQueryAsync<GetProductsByPriceQuery, ProductsResponseModel>(query,
                    cancellationToken);
            return Results.Ok(result);
        }).WithTags("Filters");

        app.MapGet(ApiRoutes.Filters.GetProductsByName,
            async (IMediator mediator, string namePart, CancellationToken cancellationToken) =>
            {
                var query = new GetProductsByNameQuery(namePart);

                var result =
                    await mediator.ExecuteQueryAsync<GetProductsByNameQuery, ProductsResponseModel>(query,
                        cancellationToken);
                return Results.Ok(result);
            }).WithTags("Filters");

        return app;
    }
}