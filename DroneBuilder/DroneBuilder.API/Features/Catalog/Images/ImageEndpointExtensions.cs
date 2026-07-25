using DroneBuilder.API.Authorization;
using DroneBuilder.API.Endpoints.Routes;
using DroneBuilder.API.Extensions;
using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Features.Catalog.Images.DeleteImage;
using DroneBuilder.Application.Features.Catalog.Images.GetImageById;
using DroneBuilder.Application.Features.Catalog.Images.GetImages;
using DroneBuilder.Application.Features.Catalog.Images.GetImagesByProductId;
using DroneBuilder.Application.Features.Catalog.Images.SetPrimaryImage;
using DroneBuilder.Application.Features.Catalog.Images.UploadImage;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace DroneBuilder.API.Features.Catalog.Images;

public static class ImageEndpointExtensions
{
    public static IEndpointRouteBuilder MapImageEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.Images.Upload,
                async (IMediator mediator, IFormFile file, [FromQuery] Guid productId, CancellationToken cancellationToken) =>
                {
                    if (file.Length == 0)
                    {
                        return Results.BadRequest("File is empty");
                    }

                    await using Stream content = file.OpenReadStream();
                    var upload = new FileUpload(content, file.FileName, file.ContentType, file.Length);
                    var command = new UploadImageCommand(upload, productId);

                    Result<ImageModel> result =
                        await mediator.ExecuteCommandAsync<UploadImageCommand, ImageModel>(
                            command,
                            cancellationToken);

                    return result.ToHttpResult();
                })
            .WithTags("Images")
            .DisableAntiforgery()
            .RequireAuthorization(PolicyNames.Admin);

        app.MapDelete(ApiRoutes.Images.Delete,
                async (IMediator mediator, Guid imageId, CancellationToken cancellationToken) =>
                {
                    var command = new DeleteImageCommand(imageId);

                    Result result = await mediator.ExecuteCommandAsync(command, cancellationToken);

                    return result.ToHttpResult();
                })
            .WithTags("Images")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapGet(ApiRoutes.Images.GetAll,
                async (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var query = new GetImagesQuery();

                    Result<ICollection<ImageModel>> result =
                        await mediator.ExecuteQueryAsync<GetImagesQuery, ICollection<ImageModel>>(
                            query,
                            cancellationToken);

                    return result.ToHttpResult();
                })
            .WithTags("Images")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapGet(ApiRoutes.Images.GetImagesByProductId,
                async (IMediator mediator, Guid productId, CancellationToken cancellationToken) =>
                {
                    var query = new GetImagesByProductIdQuery(productId);

                    Result<ICollection<ImageModel>> result =
                        await mediator.ExecuteQueryAsync<GetImagesByProductIdQuery, ICollection<ImageModel>>(
                            query,
                            cancellationToken);

                    return result.ToHttpResult();
                })
            .WithTags("Images")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapGet(ApiRoutes.Images.GetById,
                async (IMediator mediator, Guid imageId, CancellationToken cancellationToken) =>
                {
                    var query = new GetImageByIdQuery(imageId);

                    Result<ImageModel> result =
                        await mediator.ExecuteQueryAsync<GetImageByIdQuery, ImageModel>(
                            query,
                            cancellationToken);

                    return result.ToHttpResult();
                })
            .WithTags("Images")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapPost(ApiRoutes.Images.SetPrimary,
                async (IMediator mediator, Guid imageId, CancellationToken cancellationToken) =>
                {
                    var command = new SetPrimaryImageCommand(imageId);

                    Result result = await mediator.ExecuteCommandAsync(command, cancellationToken);

                    return result.ToHttpResult();
                })
            .WithTags("Images")
            .RequireAuthorization(PolicyNames.Admin);

        return app;
    }
}
