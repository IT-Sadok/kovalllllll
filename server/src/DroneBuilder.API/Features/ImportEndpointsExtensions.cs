using DroneBuilder.API.Common.Authorization;
using DroneBuilder.API.Common.Extensions;
using DroneBuilder.API.Common.Routes;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Features.Imports;
using DroneBuilder.Application.Features.Imports.GetImportRunById;
using DroneBuilder.Application.Features.Imports.GetImportRuns;
using DroneBuilder.Application.Features.Imports.StartRaceDayQuadsImport;
using FluentResults;

namespace DroneBuilder.API.Features;

public static class ImportEndpointsExtensions
{
    public static IEndpointRouteBuilder MapImportEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.Imports.StartRaceDayQuads,
                async (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ImportRunModel> result =
                        await mediator.ExecuteCommandAsync<StartRaceDayQuadsImportCommand, ImportRunModel>(
                            new StartRaceDayQuadsImportCommand(), cancellationToken);
                    return result.ToHttpResult();
                }).WithTags("Imports")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapGet(ApiRoutes.Imports.GetAll,
                async (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ICollection<ImportRunModel>> result =
                        await mediator.ExecuteQueryAsync<GetImportRunsQuery, ICollection<ImportRunModel>>(
                            new GetImportRunsQuery(), cancellationToken);
                    return result.ToHttpResult();
                }).WithTags("Imports")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapGet(ApiRoutes.Imports.GetById,
                async (Guid importRunId, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ImportRunModel> result = await mediator.ExecuteQueryAsync<GetImportRunByIdQuery, ImportRunModel>(
                        new GetImportRunByIdQuery(importRunId), cancellationToken);
                    return result.ToHttpResult();
                }).WithTags("Imports")
            .RequireAuthorization(PolicyNames.Admin);

        return app;
    }
}
