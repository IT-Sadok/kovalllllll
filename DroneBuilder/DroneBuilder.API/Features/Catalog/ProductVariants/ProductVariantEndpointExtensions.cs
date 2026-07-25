using DroneBuilder.API.Authorization;
using DroneBuilder.API.Endpoints.Routes;
using DroneBuilder.API.Extensions;
using DroneBuilder.Application.Features.Catalog.ProductVariants.CreateProductVariant;
using DroneBuilder.Application.Features.Catalog.ProductVariants.CreateVariantSpecification;
using DroneBuilder.Application.Features.Catalog.ProductVariants.DeleteProductVariant;
using DroneBuilder.Application.Features.Catalog.ProductVariants.DeleteVariantSpecification;
using DroneBuilder.Application.Features.Catalog.ProductVariants.GetAdminProductVariants;
using DroneBuilder.Application.Features.Catalog.ProductVariants.GetProductVariants;
using DroneBuilder.Application.Features.Catalog.ProductVariants.GetVariantSpecifications;
using DroneBuilder.Application.Features.Catalog.ProductVariants.Models;
using DroneBuilder.Application.Features.Catalog.ProductVariants.UpdateProductVariant;
using DroneBuilder.Application.Features.Catalog.ProductVariants.UpdateVariantSpecification;
using DroneBuilder.Application.Mediator.Interfaces;
using FluentResults;

namespace DroneBuilder.API.Features.Catalog.ProductVariants;

public static class ProductVariantEndpointExtensions
{
    public static IEndpointRouteBuilder MapProductVariantEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRoutes.AdminProducts.Variants,
                async (Guid productId, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ICollection<ProductVariantModel>> result =
                        await mediator.ExecuteQueryAsync<
                            GetAdminProductVariantsQuery,
                            ICollection<ProductVariantModel>>(
                            new GetAdminProductVariantsQuery(productId), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Admin Products")
            .RequireAuthorization(PolicyNames.Admin);
        app.MapGet(ApiRoutes.ProductVariants.GetAll,
                async (Guid productId, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ICollection<ProductVariantModel>> result =
                        await mediator.ExecuteQueryAsync<GetProductVariantsQuery, ICollection<ProductVariantModel>>(
                            new GetProductVariantsQuery(productId), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Product Variants");

        app.MapPost(ApiRoutes.ProductVariants.Create,
                async (Guid productId, CreateProductVariantModel model,
                    IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ProductVariantModel> result =
                        await mediator.ExecuteCommandAsync<CreateProductVariantCommand, ProductVariantModel>(
                            new CreateProductVariantCommand(productId, model), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Product Variants")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapPatch(ApiRoutes.ProductVariants.ById,
                async (Guid productId, Guid variantId, UpdateProductVariantModel model,
                    IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ProductVariantModel> result =
                        await mediator.ExecuteCommandAsync<UpdateProductVariantCommand, ProductVariantModel>(
                            new UpdateProductVariantCommand(productId, variantId, model), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Product Variants")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapDelete(ApiRoutes.ProductVariants.ById,
                async (Guid productId, Guid variantId,
                    IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result result = await mediator.ExecuteCommandAsync(
                        new DeleteProductVariantCommand(productId, variantId), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Product Variants")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapGet(ApiRoutes.ProductVariants.Specifications,
                async (Guid productId, Guid variantId,
                    IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ICollection<ProductVariantSpecificationModel>> result =
                        await mediator.ExecuteQueryAsync<
                            GetVariantSpecificationsQuery,
                            ICollection<ProductVariantSpecificationModel>>(
                            new GetVariantSpecificationsQuery(productId, variantId), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Product Variant Specifications");

        app.MapPost(ApiRoutes.ProductVariants.Specifications,
                async (Guid productId, Guid variantId, CreateProductVariantSpecificationModel model,
                    IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ProductVariantSpecificationModel> result =
                        await mediator.ExecuteCommandAsync<
                            CreateVariantSpecificationCommand,
                            ProductVariantSpecificationModel>(
                            new CreateVariantSpecificationCommand(productId, variantId, model), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Product Variant Specifications")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapPut(ApiRoutes.ProductVariants.SpecificationById,
                async (Guid productId, Guid variantId, Guid specificationId,
                    UpdateProductVariantSpecificationModel model,
                    IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ProductVariantSpecificationModel> result =
                        await mediator.ExecuteCommandAsync<
                            UpdateVariantSpecificationCommand,
                            ProductVariantSpecificationModel>(
                            new UpdateVariantSpecificationCommand(
                                productId, variantId, specificationId, model),
                            cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Product Variant Specifications")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapDelete(ApiRoutes.ProductVariants.SpecificationById,
                async (Guid productId, Guid variantId, Guid specificationId,
                    IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result result = await mediator.ExecuteCommandAsync(
                        new DeleteVariantSpecificationCommand(productId, variantId, specificationId),
                        cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Product Variant Specifications")
            .RequireAuthorization(PolicyNames.Admin);

        return app;
    }
}
