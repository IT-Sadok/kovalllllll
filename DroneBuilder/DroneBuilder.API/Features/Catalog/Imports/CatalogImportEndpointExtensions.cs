using DroneBuilder.API.Authorization;
using DroneBuilder.API.Endpoints.Routes;
using DroneBuilder.API.Extensions;
using DroneBuilder.Application.Features.Catalog.Imports.CreateImportSource;
using DroneBuilder.Application.Features.Catalog.Imports.DeleteImportSource;
using DroneBuilder.Application.Features.Catalog.Imports.DeleteProductExternalReference;
using DroneBuilder.Application.Features.Catalog.Imports.DeleteVariantExternalReference;
using DroneBuilder.Application.Features.Catalog.Imports.GetImportBatches;
using DroneBuilder.Application.Features.Catalog.Imports.GetImportItems;
using DroneBuilder.Application.Features.Catalog.Imports.GetImportSources;
using DroneBuilder.Application.Features.Catalog.Imports.GetProductExternalReferences;
using DroneBuilder.Application.Features.Catalog.Imports.GetVariantExternalReferences;
using DroneBuilder.Application.Features.Catalog.Imports.Models;
using DroneBuilder.Application.Features.Catalog.Imports.ReplacePropertyAliases;
using DroneBuilder.Application.Features.Catalog.Imports.ReplaceValueAliases;
using DroneBuilder.Application.Features.Catalog.Imports.UpdateImportSource;
using DroneBuilder.Application.Features.Catalog.Imports.UpsertProductExternalReference;
using DroneBuilder.Application.Features.Catalog.Imports.UpsertVariantExternalReference;
using DroneBuilder.Application.Mediator.Interfaces;
using FluentResults;

namespace DroneBuilder.API.Features.Catalog.Imports;

public static class CatalogImportEndpointExtensions
{
    public static IEndpointRouteBuilder MapCatalogImportEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRoutes.Imports.Sources,
                async (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ICollection<ImportSourceModel>> result =
                        await mediator.ExecuteQueryAsync<GetImportSourcesQuery, ICollection<ImportSourceModel>>(
                            new GetImportSourcesQuery(), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Catalog Imports")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapPost(ApiRoutes.Imports.Sources,
                async (CreateImportSourceModel model, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ImportSourceModel> result =
                        await mediator.ExecuteCommandAsync<CreateImportSourceCommand, ImportSourceModel>(
                            new CreateImportSourceCommand(model), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Catalog Imports")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapPatch(ApiRoutes.Imports.SourceById,
                async (Guid sourceId, UpdateImportSourceModel model,
                    IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ImportSourceModel> result =
                        await mediator.ExecuteCommandAsync<UpdateImportSourceCommand, ImportSourceModel>(
                            new UpdateImportSourceCommand(sourceId, model), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Catalog Imports")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapDelete(ApiRoutes.Imports.SourceById,
                async (Guid sourceId, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result result = await mediator.ExecuteCommandAsync(
                        new DeleteImportSourceCommand(sourceId), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Catalog Imports")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapGet(ApiRoutes.Imports.Batches,
                async (Guid sourceId, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ICollection<ImportBatchModel>> result =
                        await mediator.ExecuteQueryAsync<GetImportBatchesQuery, ICollection<ImportBatchModel>>(
                            new GetImportBatchesQuery(sourceId), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Catalog Imports")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapGet(ApiRoutes.Imports.Items,
                async (Guid sourceId, Guid batchId, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ICollection<ImportItemModel>> result =
                        await mediator.ExecuteQueryAsync<GetImportItemsQuery, ICollection<ImportItemModel>>(
                            new GetImportItemsQuery(sourceId, batchId), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Catalog Imports")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapPut(ApiRoutes.Imports.PropertyAliases,
                async (Guid sourceId, Guid propertyId, ReplaceSourceAliasesModel model,
                    IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ICollection<string>> result =
                        await mediator.ExecuteCommandAsync<ReplacePropertyAliasesCommand, ICollection<string>>(
                            new ReplacePropertyAliasesCommand(sourceId, propertyId, model), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Catalog Imports")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapPut(ApiRoutes.Imports.ValueAliases,
                async (Guid sourceId, Guid valueId, ReplaceSourceAliasesModel model,
                    IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ICollection<string>> result =
                        await mediator.ExecuteCommandAsync<ReplaceValueAliasesCommand, ICollection<string>>(
                            new ReplaceValueAliasesCommand(sourceId, valueId, model), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Catalog Imports")
            .RequireAuthorization(PolicyNames.Admin);

        MapProductReferenceEndpoints(app);
        MapVariantReferenceEndpoints(app);
        return app;
    }

    private static void MapProductReferenceEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRoutes.Imports.ProductReferences,
                async (Guid productId, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ICollection<ExternalReferenceModel>> result =
                        await mediator.ExecuteQueryAsync<
                            GetProductExternalReferencesQuery,
                            ICollection<ExternalReferenceModel>>(
                            new GetProductExternalReferencesQuery(productId), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Catalog Imports")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapPut(ApiRoutes.Imports.ProductReferenceBySource,
                async (Guid productId, Guid sourceId, UpsertExternalReferenceModel model,
                    IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ExternalReferenceModel> result =
                        await mediator.ExecuteCommandAsync<
                            UpsertProductExternalReferenceCommand,
                            ExternalReferenceModel>(
                            new UpsertProductExternalReferenceCommand(productId, sourceId, model),
                            cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Catalog Imports")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapDelete(ApiRoutes.Imports.ProductReferenceById,
                async (Guid productId, Guid referenceId,
                    IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result result = await mediator.ExecuteCommandAsync(
                        new DeleteProductExternalReferenceCommand(productId, referenceId), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Catalog Imports")
            .RequireAuthorization(PolicyNames.Admin);
    }

    private static void MapVariantReferenceEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRoutes.Imports.VariantReferences,
                async (Guid productId, Guid variantId,
                    IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ICollection<ExternalReferenceModel>> result =
                        await mediator.ExecuteQueryAsync<
                            GetVariantExternalReferencesQuery,
                            ICollection<ExternalReferenceModel>>(
                            new GetVariantExternalReferencesQuery(productId, variantId), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Catalog Imports")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapPut(ApiRoutes.Imports.VariantReferenceBySource,
                async (Guid productId, Guid variantId, Guid sourceId, UpsertExternalReferenceModel model,
                    IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ExternalReferenceModel> result =
                        await mediator.ExecuteCommandAsync<
                            UpsertVariantExternalReferenceCommand,
                            ExternalReferenceModel>(
                            new UpsertVariantExternalReferenceCommand(
                                productId, variantId, sourceId, model),
                            cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Catalog Imports")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapDelete(ApiRoutes.Imports.VariantReferenceById,
                async (Guid productId, Guid variantId, Guid referenceId,
                    IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result result = await mediator.ExecuteCommandAsync(
                        new DeleteVariantExternalReferenceCommand(productId, variantId, referenceId),
                        cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Catalog Imports")
            .RequireAuthorization(PolicyNames.Admin);
    }
}
