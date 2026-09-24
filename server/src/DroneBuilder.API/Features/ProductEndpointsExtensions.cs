using DroneBuilder.API.Common.Authorization;
using DroneBuilder.API.Common.Extensions;
using DroneBuilder.API.Common.Routes;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Pagination;
using DroneBuilder.Application.Features.Products;
using DroneBuilder.Application.Features.Products.CreateProduct;
using DroneBuilder.Application.Features.Products.DeleteProduct;
using DroneBuilder.Application.Features.Products.GetCategories;
using DroneBuilder.Application.Features.Products.GetDelistedProducts;
using DroneBuilder.Application.Features.Products.GetProductById;
using DroneBuilder.Application.Features.Products.GetProducts;
using DroneBuilder.Application.Features.Products.RemoveProductSpec;
using DroneBuilder.Application.Features.Products.RestoreProduct;
using DroneBuilder.Application.Features.Products.SetProductAttributes;
using DroneBuilder.Application.Features.Products.SetProductSpec;
using DroneBuilder.Application.Features.Products.UpdateProduct;
using FluentResults;

namespace DroneBuilder.API.Features;

public static class ProductEndpointsExtensions
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.Products.Create,
                async (IMediator mediator, CreateProductModel model, CancellationToken cancellationToken) =>
                {
                    Result<ProductModel> result = await mediator.ExecuteCommandAsync<CreateProductCommand, ProductModel>(
                        new CreateProductCommand(model),
                        cancellationToken);
                    return result.ToHttpResult();
                }).WithTags("Products")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapPatch(ApiRoutes.Products.Update,
                async (Guid productId, UpdateProductRequestModel requestModel,
                    IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ProductModel> result = await mediator.ExecuteCommandAsync<UpdateProductCommand, ProductModel>(
                        new UpdateProductCommand(productId, requestModel),
                        cancellationToken);
                    return result.ToHttpResult();
                }).WithTags("Products")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapGet(ApiRoutes.Products.GetDelisted,
                async (int page, int pageSize, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var pagination = new PaginationParams(page, pageSize);

                    Result<PagedResult<ProductModel>> result =
                        await mediator.ExecuteQueryAsync<GetDelistedProductsQuery, PagedResult<ProductModel>>(
                            new GetDelistedProductsQuery(pagination),
                            cancellationToken);
                    return result.ToHttpResult();
                }).WithTags("Products")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapPost(ApiRoutes.Products.Restore, async (IMediator mediator, Guid productId,
                CancellationToken cancellationToken) =>
            {
                Result result = await mediator.ExecuteCommandAsync(
                    new RestoreProductCommand(productId), cancellationToken);
                return result.ToHttpResult();
            }).WithTags("Products")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapDelete(ApiRoutes.Products.Delete, async (IMediator mediator, Guid productId,
                CancellationToken cancellationToken) =>
            {
                Result result = await mediator.ExecuteCommandAsync(new DeleteProductCommand(productId), cancellationToken);
                return result.ToHttpResult();
            }).WithTags("Products")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapGet(ApiRoutes.Products.GetAll,
                async (int page, int pageSize, IMediator mediator, [AsParameters] ProductFilterModel filter,
                    CancellationToken cancellationToken) =>
                {
                    var pagination = new PaginationParams(page, pageSize);
                    var query = new GetProductsQuery(pagination, filter);
                    Result<PagedResult<ProductModel>> result = await mediator.ExecuteQueryAsync<GetProductsQuery, PagedResult<ProductModel>>(
                        query,
                        cancellationToken);
                    return result.ToHttpResult();
                }).WithTags("Products");

        app.MapGet(ApiRoutes.Products.GetCategories,
                async (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<IEnumerable<string>> result = await mediator.ExecuteQueryAsync<GetCategoriesQuery, IEnumerable<string>>(
                        new GetCategoriesQuery(),
                        cancellationToken);
                    return result.ToHttpResult();
                }).WithTags("Products");

        app.MapGet(ApiRoutes.Products.GetById,
                async (IMediator mediator, Guid productId, CancellationToken cancellationToken) =>
                {
                    Result<ProductModel> result = await mediator.ExecuteQueryAsync<GetProductByIdQuery, ProductModel>(
                        new GetProductByIdQuery(productId),
                        cancellationToken);
                    return result.ToHttpResult();
                }).WithTags("Products");

        app.MapPut(ApiRoutes.Products.SetSpec,
                async (Guid productId, ComponentSpecModel spec, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ProductModel> result = await mediator.ExecuteCommandAsync<SetProductSpecCommand, ProductModel>(
                        new SetProductSpecCommand(productId, spec),
                        cancellationToken);
                    return result.ToHttpResult();
                }).WithTags("Products")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapDelete(ApiRoutes.Products.RemoveSpec, async (IMediator mediator, Guid productId,
                CancellationToken cancellationToken) =>
            {
                Result result = await mediator.ExecuteCommandAsync(new RemoveProductSpecCommand(productId),
                    cancellationToken);
                return result.ToHttpResult();
            }).WithTags("Products")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapPut(ApiRoutes.Products.SetAttributes,
                async (Guid productId, List<ProductAttributeModel> attributes, IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    Result<ProductModel> result =
                        await mediator.ExecuteCommandAsync<SetProductAttributesCommand, ProductModel>(
                            new SetProductAttributesCommand(productId, attributes),
                            cancellationToken);
                    return result.ToHttpResult();
                }).WithTags("Products")
            .RequireAuthorization(PolicyNames.Admin);

        return app;
    }
}
