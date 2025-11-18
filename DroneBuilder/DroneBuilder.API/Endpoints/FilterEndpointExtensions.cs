using DroneBuilder.API.Endpoints.Routes;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Mediator.Queries.Filters;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DroneBuilder.API.Endpoints;

public static class FilterEndpointExtensions
{
    public static IEndpointRouteBuilder MapFilterEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRoutes.Filters.GetFilteredProducts,
                async (IMediator mediator, [AsParameters] ProductFilterModel filter,
                    CancellationToken cancellationToken) =>
                {
                    var query = new GetFilteredProductsQuery(filter);
                    var result = await mediator.ExecuteQueryAsync<GetFilteredProductsQuery, ProductsResponseModel>(
                        query, cancellationToken);

                    return Results.Ok(result);
                })
            .WithTags("Filters");


        return app;
    }
}