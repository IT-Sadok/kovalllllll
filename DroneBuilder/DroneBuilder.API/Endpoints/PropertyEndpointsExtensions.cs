using DroneBuilder.API.Endpoints.Routes;
using DroneBuilder.Application.Mediator.Commands.PropertyCommands;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Mediator.Queries.PropertyQueries;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.API.Endpoints;

public static class PropertyEndpointsExtensions
{
    public static IEndpointRouteBuilder MapPropertyEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.Properties.Create,
                async (IMediator mediator, CreatePropertyModel model, CancellationToken cancellationToken) =>
                {
                    var result = await mediator.ExecuteCommandAsync<CreatePropertyCommand, PropertyResponseModel>(
                        new CreatePropertyCommand(model),
                        cancellationToken);
                    return Results.Ok(result);
                })
            .WithTags("Properties");

        app.MapDelete(ApiRoutes.Properties.Delete,
            async (IMediator mediator, Guid propertyId, CancellationToken cancellationToken) =>
            {
                await mediator.ExecuteCommandAsync(new DeletePropertyCommand(propertyId), cancellationToken);
                return Results.NoContent();
            }).WithTags("Properties");

        app.MapPatch(ApiRoutes.Properties.Update, async (IMediator mediator, Guid propertyId,
            UpdatePropertyModel model, CancellationToken cancellationToken) =>
        {
            var result = await mediator.ExecuteCommandAsync<UpdatePropertyCommand, PropertyResponseModel>(
                new UpdatePropertyCommand(propertyId, model),
                cancellationToken);
            return Results.Ok(result);
        }).WithTags("Properties");

        app.MapGet(ApiRoutes.Properties.GetAll,
                async (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var result = await mediator.ExecuteQueryAsync<GetPropertiesQuery, PropertiesResponseModel>(
                        new GetPropertiesQuery(),
                        cancellationToken);
                    return Results.Ok(result);
                })
            .WithTags("Properties");

        app.MapGet(ApiRoutes.Properties.GetValuesByPropertyId,
            async (IMediator mediator, Guid propertyId, CancellationToken cancellationToken) =>
            {
                var result = await mediator.ExecuteQueryAsync<GetValuesByPropertyIdQuery, PropertyResponseModel>(
                    new GetValuesByPropertyIdQuery(propertyId),
                    cancellationToken);
                return Results.Ok(result);
            }).WithTags("Properties");

        return app;
    }
}