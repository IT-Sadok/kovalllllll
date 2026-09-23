using DroneBuilder.API.Authorization;
using DroneBuilder.API.Endpoints.Routes;
using DroneBuilder.API.Extensions;
using DroneBuilder.Application.Mediator.Commands.PropertyCommands;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Mediator.Queries.PropertyQueries;
using DroneBuilder.Application.Models.ProductModels;
using FluentResults;

namespace DroneBuilder.API.Endpoints;

public static class PropertyEndpointsExtensions
{
    public static IEndpointRouteBuilder MapPropertyEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.Properties.Create,
                async (IMediator mediator, CreatePropertyModel model, CancellationToken cancellationToken) =>
                {
                    Result<PropertyModel> result = await mediator.ExecuteCommandAsync<CreatePropertyCommand, PropertyModel>(
                        new CreatePropertyCommand(model),
                        cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Properties")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapDelete(ApiRoutes.Properties.Delete,
                async (IMediator mediator, Guid propertyId, CancellationToken cancellationToken) =>
                {
                    Result result = await mediator.ExecuteCommandAsync(new DeletePropertyCommand(propertyId), cancellationToken);
                    return result.ToHttpResult();
                }).WithTags("Properties")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapPatch(ApiRoutes.Properties.Update, async (IMediator mediator, Guid propertyId,
                UpdatePropertyModel model, CancellationToken cancellationToken) =>
            {
                Result<PropertyModel> result = await mediator.ExecuteCommandAsync<UpdatePropertyCommand, PropertyModel>(
                    new UpdatePropertyCommand(propertyId, model),
                    cancellationToken);
                return result.ToHttpResult();
            }).WithTags("Properties")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapGet(ApiRoutes.Properties.GetAll,
                async (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ICollection<PropertyModel>> result = await mediator.ExecuteQueryAsync<GetPropertiesQuery, ICollection<PropertyModel>>(
                        new GetPropertiesQuery(),
                        cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Properties")
            .RequireAuthorization();

        app.MapGet(ApiRoutes.Properties.GetValuesByPropertyId,
                async (IMediator mediator, Guid propertyId, CancellationToken cancellationToken) =>
                {
                    Result<PropertyModel> result = await mediator.ExecuteQueryAsync<GetValuesByPropertyIdQuery, PropertyModel>(
                        new GetValuesByPropertyIdQuery(propertyId),
                        cancellationToken);
                    return result.ToHttpResult();
                }).WithTags("Properties")
            .RequireAuthorization();

        app.MapPost(ApiRoutes.Properties.AssignValueToProperty, async (IMediator mediator, Guid propertyId, Guid valueId,
                CancellationToken cancellationToken) =>
            {
                Result result = await mediator.ExecuteCommandAsync(new AddValueToPropertyCommand(propertyId, valueId),
                    cancellationToken);
                return result.ToHttpResult();
            }).WithTags("Properties")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapDelete(ApiRoutes.Properties.RemoveValueFromProperty, async (IMediator mediator, Guid propertyId, Guid valueId,
                CancellationToken cancellationToken) =>
            {
                Result result = await mediator.ExecuteCommandAsync(new RemoveValueFromPropertyCommand(propertyId, valueId),
                    cancellationToken);
                return result.ToHttpResult();
            }).WithTags("Properties")
            .RequireAuthorization(PolicyNames.Admin);

        return app;
    }
}
