using DroneBuilder.API.Common.Extensions;
using DroneBuilder.API.Common.Routes;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Features.Builds.CheckBuild;
using FluentResults;

namespace DroneBuilder.API.Features;

public static class BuildEndpointsExtensions
{
    public static IEndpointRouteBuilder MapBuildEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.Builds.Check,
                async (CheckBuildQuery query, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<BuildCheckModel> result =
                        await mediator.ExecuteQueryAsync<CheckBuildQuery, BuildCheckModel>(query, cancellationToken);
                    return result.ToHttpResult();
                }).WithTags("Builds");

        return app;
    }
}
