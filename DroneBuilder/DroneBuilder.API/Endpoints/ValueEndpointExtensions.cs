using DroneBuilder.API.Endpoints.Routes;
using DroneBuilder.Application.Mediator.Commands.ValueCommands;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Mediator.Queries.ValueQueries;
using DroneBuilder.Application.Models.ProductModels;

namespace DroneBuilder.API.Endpoints;

public static class ValueEndpointExtensions
{
    public static IEndpointRouteBuilder MapValueEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.Values.Create,
                async (IMediator mediator, CreateValueModel model, CancellationToken cancellationToken) =>
                {
                    var result = await mediator.ExecuteCommandAsync<CreateValueCommand, ValueModel>(
                        new CreateValueCommand(model),
                        cancellationToken);
                    return Results.Ok(result);
                })
            .WithTags("Values")
            .RequireAuthorization();

        app.MapDelete(ApiRoutes.Values.Delete,
                async (IMediator mediator, Guid valueId, CancellationToken cancellationToken) =>
                {
                    await mediator.ExecuteCommandAsync(new DeleteValueCommand(valueId), cancellationToken);
                    return Results.NoContent();
                }).WithTags("Values")
            .RequireAuthorization();

        app.MapPatch(ApiRoutes.Values.Update, async (IMediator mediator, Guid valueId,
                UpdateValueModel model, CancellationToken cancellationToken) =>
            {
                var result = await mediator.ExecuteCommandAsync<UpdateValueCommand, ValueModel>(
                    new UpdateValueCommand(valueId, model),
                    cancellationToken);
                return Results.Ok(result);
            }).WithTags("Values")
            .RequireAuthorization();

        app.MapGet(ApiRoutes.Values.GetAll,
                async (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var result = await mediator.ExecuteQueryAsync<GetValuesQuery, ICollection<ValueModel>>(
                        new GetValuesQuery(),
                        cancellationToken);
                    return Results.Ok(result);
                })
            .WithTags("Values")
            .RequireAuthorization();


        app.MapGet(ApiRoutes.Values.GetById,
                async (IMediator mediator, Guid valueId, CancellationToken cancellationToken) =>
                {
                    var result = await mediator.ExecuteQueryAsync<GetValueByIdQuery, ValueModel>(
                        new GetValueByIdQuery(valueId),
                        cancellationToken);
                    return Results.Ok(result);
                }).WithTags("Values")
            .RequireAuthorization();

        return app;
    }
}