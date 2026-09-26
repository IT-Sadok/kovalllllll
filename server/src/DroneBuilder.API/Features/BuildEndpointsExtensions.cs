using DroneBuilder.API.Common.Extensions;
using DroneBuilder.API.Common.Routes;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Features.Builds.CheckBuild;
using DroneBuilder.Application.Features.Builds.SavedBuilds;
using DroneBuilder.Application.Features.Builds.SavedBuilds.CreateBuild;
using DroneBuilder.Application.Features.Builds.SavedBuilds.DeleteBuild;
using DroneBuilder.Application.Features.Builds.SavedBuilds.GetBuildById;
using DroneBuilder.Application.Features.Builds.SavedBuilds.GetBuilds;
using DroneBuilder.Application.Features.Builds.SavedBuilds.UpdateBuild;
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

        app.MapGet(ApiRoutes.Builds.GetAll,
                async (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ICollection<SavedBuildModel>> result =
                        await mediator.ExecuteQueryAsync<GetBuildsQuery, ICollection<SavedBuildModel>>(
                            new GetBuildsQuery(), cancellationToken);
                    return result.ToHttpResult();
                }).WithTags("Builds")
            .RequireAuthorization();

        app.MapGet(ApiRoutes.Builds.GetById,
                async (Guid buildId, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<SavedBuildModel> result =
                        await mediator.ExecuteQueryAsync<GetBuildByIdQuery, SavedBuildModel>(
                            new GetBuildByIdQuery(buildId), cancellationToken);
                    return result.ToHttpResult();
                }).WithTags("Builds")
            .RequireAuthorization();

        app.MapPost(ApiRoutes.Builds.Create,
                async (SaveBuildModel model, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<SavedBuildModel> result =
                        await mediator.ExecuteCommandAsync<CreateBuildCommand, SavedBuildModel>(
                            new CreateBuildCommand(model), cancellationToken);
                    return result.ToHttpResult();
                }).WithTags("Builds")
            .RequireAuthorization();

        app.MapPut(ApiRoutes.Builds.Update,
                async (Guid buildId, SaveBuildModel model, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<SavedBuildModel> result =
                        await mediator.ExecuteCommandAsync<UpdateBuildCommand, SavedBuildModel>(
                            new UpdateBuildCommand(buildId, model), cancellationToken);
                    return result.ToHttpResult();
                }).WithTags("Builds")
            .RequireAuthorization();

        app.MapDelete(ApiRoutes.Builds.Delete,
                async (Guid buildId, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result result = await mediator.ExecuteCommandAsync(new DeleteBuildCommand(buildId), cancellationToken);
                    return result.ToHttpResult();
                }).WithTags("Builds")
            .RequireAuthorization();

        return app;
    }
}
