using DroneBuilder.API.Authorization;
using DroneBuilder.API.Endpoints.Routes;
using DroneBuilder.API.Extensions;
using DroneBuilder.Application.Mediator.Commands.ImageCommands;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Mediator.Queries.ImageQueries;
using DroneBuilder.Application.Models.ProductModels;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace DroneBuilder.API.Endpoints;

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

                    var command = new UploadImageCommand(file, productId);

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
