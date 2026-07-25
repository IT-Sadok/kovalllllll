using DroneBuilder.API.Authorization;
using DroneBuilder.API.Endpoints.Routes;
using DroneBuilder.API.Extensions;
using DroneBuilder.Application.Features.Catalog.Products.AssignComponentType;
using DroneBuilder.Application.Features.Catalog.ProductSpecifications.CreateProductSpecification;
using DroneBuilder.Application.Features.Catalog.ProductSpecifications.DeleteProductSpecification;
using DroneBuilder.Application.Features.Catalog.ProductSpecifications.GetAdminProductSpecifications;
using DroneBuilder.Application.Features.Catalog.ProductSpecifications.GetProductSpecifications;
using DroneBuilder.Application.Features.Catalog.ProductSpecifications.Models;
using DroneBuilder.Application.Features.Catalog.ProductSpecifications.UpdateProductSpecification;
using DroneBuilder.Application.Mediator.Interfaces;
using FluentResults;

namespace DroneBuilder.API.Features.Catalog.ProductSpecifications;

public static class ProductSpecificationEndpointExtensions
{
    public static IEndpointRouteBuilder MapProductSpecificationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRoutes.Products.Specifications,
                async (Guid productId, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ICollection<ProductSpecificationModel>> result =
                        await mediator.ExecuteQueryAsync<
                            GetProductSpecificationsQuery,
                            ICollection<ProductSpecificationModel>>(
                            new GetProductSpecificationsQuery(productId),
                            cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Product Specifications");
        app.MapGet(ApiRoutes.AdminProducts.Specifications,
                async (Guid productId, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ICollection<ProductSpecificationModel>> result =
                        await mediator.ExecuteQueryAsync<
                            GetAdminProductSpecificationsQuery,
                            ICollection<ProductSpecificationModel>>(
                            new GetAdminProductSpecificationsQuery(productId),
                            cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Product Specifications")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapPut(ApiRoutes.Products.AssignComponentType,
                async (Guid productId, AssignProductComponentTypeModel model,
                    IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ProductComponentTypeModel> result =
                        await mediator.ExecuteCommandAsync<
                            AssignProductComponentTypeCommand,
                            ProductComponentTypeModel>(
                            new AssignProductComponentTypeCommand(productId, model),
                            cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Product Specifications")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapPost(ApiRoutes.Products.Specifications,
                async (Guid productId, CreateProductSpecificationModel model,
                    IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ProductSpecificationModel> result =
                        await mediator.ExecuteCommandAsync<
                            CreateProductSpecificationCommand,
                            ProductSpecificationModel>(
                            new CreateProductSpecificationCommand(productId, model),
                            cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Product Specifications")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapPut(ApiRoutes.Products.SpecificationById,
                async (Guid productId, Guid specificationId, UpdateProductSpecificationModel model,
                    IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ProductSpecificationModel> result =
                        await mediator.ExecuteCommandAsync<
                            UpdateProductSpecificationCommand,
                            ProductSpecificationModel>(
                            new UpdateProductSpecificationCommand(productId, specificationId, model),
                            cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Product Specifications")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapDelete(ApiRoutes.Products.SpecificationById,
                async (Guid productId, Guid specificationId,
                    IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result result = await mediator.ExecuteCommandAsync(
                        new DeleteProductSpecificationCommand(productId, specificationId),
                        cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Product Specifications")
            .RequireAuthorization(PolicyNames.Admin);

        return app;
    }
}
