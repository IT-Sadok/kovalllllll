using DroneBuilder.API.Authorization;
using DroneBuilder.API.Endpoints.Routes;
using DroneBuilder.API.Extensions;
using DroneBuilder.Application.Features.Catalog.Metadata.CreateComponentType;
using DroneBuilder.Application.Features.Catalog.Metadata.CreateUnit;
using DroneBuilder.Application.Features.Catalog.Metadata.DeleteComponentType;
using DroneBuilder.Application.Features.Catalog.Metadata.DeleteComponentTypeProperty;
using DroneBuilder.Application.Features.Catalog.Metadata.DeleteUnit;
using DroneBuilder.Application.Features.Catalog.Metadata.GetAdminComponentType;
using DroneBuilder.Application.Features.Catalog.Metadata.GetAdminComponentTypes;
using DroneBuilder.Application.Features.Catalog.Metadata.GetComponentTypeProperties;
using DroneBuilder.Application.Features.Catalog.Metadata.GetComponentTypes;
using DroneBuilder.Application.Features.Catalog.Metadata.GetUnits;
using DroneBuilder.Application.Features.Catalog.Metadata.Models;
using DroneBuilder.Application.Features.Catalog.Metadata.UpdateComponentType;
using DroneBuilder.Application.Features.Catalog.Metadata.UpdateUnit;
using DroneBuilder.Application.Features.Catalog.Metadata.UpsertComponentTypeProperty;
using DroneBuilder.Application.Mediator.Interfaces;
using FluentResults;

namespace DroneBuilder.API.Features.Catalog.Metadata;

public static class CatalogMetadataEndpointExtensions
{
    public static IEndpointRouteBuilder MapCatalogMetadataEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRoutes.ComponentTypes.GetAll,
                async (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ICollection<ComponentTypeModel>> result =
                        await mediator.ExecuteQueryAsync<GetComponentTypesQuery, ICollection<ComponentTypeModel>>(
                            new GetComponentTypesQuery(), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Component Types");

        app.MapGet(ApiRoutes.ComponentTypes.GetProperties,
                async (Guid componentTypeId, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ComponentTypeDetailsModel> result =
                        await mediator.ExecuteQueryAsync<GetComponentTypePropertiesQuery, ComponentTypeDetailsModel>(
                            new GetComponentTypePropertiesQuery(componentTypeId), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Component Types");
        app.MapGet(ApiRoutes.AdminComponentTypes.GetAll,
                async (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ICollection<ComponentTypeModel>> result =
                        await mediator.ExecuteQueryAsync<GetAdminComponentTypesQuery, ICollection<ComponentTypeModel>>(
                            new GetAdminComponentTypesQuery(), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Component Types")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapGet(ApiRoutes.AdminComponentTypes.GetById,
                async (Guid componentTypeId, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ComponentTypeDetailsModel> result =
                        await mediator.ExecuteQueryAsync<GetAdminComponentTypeQuery, ComponentTypeDetailsModel>(
                            new GetAdminComponentTypeQuery(componentTypeId), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Component Types")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapPost(ApiRoutes.ComponentTypes.Create,
                async (CreateComponentTypeModel model, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ComponentTypeModel> result =
                        await mediator.ExecuteCommandAsync<CreateComponentTypeCommand, ComponentTypeModel>(
                            new CreateComponentTypeCommand(model), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Component Types")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapPatch(ApiRoutes.ComponentTypes.Update,
                async (Guid componentTypeId, UpdateComponentTypeModel model,
                    IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ComponentTypeModel> result =
                        await mediator.ExecuteCommandAsync<UpdateComponentTypeCommand, ComponentTypeModel>(
                            new UpdateComponentTypeCommand(componentTypeId, model), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Component Types")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapDelete(ApiRoutes.ComponentTypes.Delete,
                async (Guid componentTypeId, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result result = await mediator.ExecuteCommandAsync(
                        new DeleteComponentTypeCommand(componentTypeId), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Component Types")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapPut(ApiRoutes.ComponentTypes.PropertyRule,
                async (Guid componentTypeId, Guid propertyId, UpsertComponentTypePropertyModel model,
                    IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ComponentTypePropertyModel> result =
                        await mediator.ExecuteCommandAsync<
                            UpsertComponentTypePropertyCommand,
                            ComponentTypePropertyModel>(
                            new UpsertComponentTypePropertyCommand(componentTypeId, propertyId, model),
                            cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Component Types")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapDelete(ApiRoutes.ComponentTypes.PropertyRule,
                async (Guid componentTypeId, Guid propertyId,
                    IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result result = await mediator.ExecuteCommandAsync(
                        new DeleteComponentTypePropertyCommand(componentTypeId, propertyId), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Component Types")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapGet(ApiRoutes.Units.GetAll,
                async (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ICollection<UnitDefinitionModel>> result =
                        await mediator.ExecuteQueryAsync<GetUnitsQuery, ICollection<UnitDefinitionModel>>(
                            new GetUnitsQuery(), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Units");

        app.MapPost(ApiRoutes.Units.Create,
                async (CreateUnitDefinitionModel model, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<UnitDefinitionModel> result =
                        await mediator.ExecuteCommandAsync<CreateUnitCommand, UnitDefinitionModel>(
                            new CreateUnitCommand(model), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Units")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapPatch(ApiRoutes.Units.Update,
                async (Guid unitId, UpdateUnitDefinitionModel model,
                    IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<UnitDefinitionModel> result =
                        await mediator.ExecuteCommandAsync<UpdateUnitCommand, UnitDefinitionModel>(
                            new UpdateUnitCommand(unitId, model), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Units")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapDelete(ApiRoutes.Units.Delete,
                async (Guid unitId, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result result = await mediator.ExecuteCommandAsync(
                        new DeleteUnitCommand(unitId), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Units")
            .RequireAuthorization(PolicyNames.Admin);

        return app;
    }
}
