using DroneBuilder.API.Authorization;
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
                    var result = await mediator.ExecuteCommandAsync<CreatePropertyCommand, PropertyModel>(
                        new CreatePropertyCommand(model),
                        cancellationToken);
                    return Results.Ok(result);
                })
            .WithTags("Properties")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapDelete(ApiRoutes.Properties.Delete,
                async (IMediator mediator, Guid propertyId, CancellationToken cancellationToken) =>
                {
                    await mediator.ExecuteCommandAsync(new DeletePropertyCommand(propertyId), cancellationToken);
                    return Results.NoContent();
                }).WithTags("Properties")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapPatch(ApiRoutes.Properties.Update, async (IMediator mediator, Guid propertyId,
                UpdatePropertyModel model, CancellationToken cancellationToken) =>
            {
                var result = await mediator.ExecuteCommandAsync<UpdatePropertyCommand, PropertyModel>(
                    new UpdatePropertyCommand(propertyId, model),
                    cancellationToken);
                return Results.Ok(result);
            }).WithTags("Properties")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapGet(ApiRoutes.Properties.GetAll,
                async (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var result = await mediator.ExecuteQueryAsync<GetPropertiesQuery, ICollection<PropertyModel>>(
                        new GetPropertiesQuery(),
                        cancellationToken);
                    return Results.Ok(result);
                })
            .WithTags("Properties")
            .RequireAuthorization();

        app.MapGet(ApiRoutes.Properties.GetValuesByPropertyId,
                async (IMediator mediator, Guid propertyId, CancellationToken cancellationToken) =>
                {
                    var result = await mediator.ExecuteQueryAsync<GetValuesByPropertyIdQuery, PropertyModel>(
                        new GetValuesByPropertyIdQuery(propertyId),
                        cancellationToken);
                    return Results.Ok(result);
                }).WithTags("Properties")
            .RequireAuthorization();

        app.Map(ApiRoutes.Properties.AssignValueToProperty, async (IMediator mediator, Guid propertyId, Guid valueId,
                CancellationToken cancellationToken) =>
            {
                await mediator.ExecuteCommandAsync(new AddValueToPropertyCommand(propertyId, valueId),
                    cancellationToken);
                return Results.NoContent();
            }).WithTags("Properties")
            .RequireAuthorization(PolicyNames.Admin);

        return app;
    }
}