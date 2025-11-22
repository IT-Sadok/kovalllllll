using DroneBuilder.API.Endpoints.Routes;
using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Mediator.Commands.ImageCommands;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Mediator.Queries.ImageQueries;
using DroneBuilder.Application.Models.ProductModels;

namespace DroneBuilder.API.Endpoints;

public static class ImageEndpointExtensions
{
    public static IEndpointRouteBuilder MapImageEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.Images.Upload,
                async (IMediator mediator, IFormFile file, Guid productId, CancellationToken cancellationToken) =>
                {
                    if (file.Length == 0)
                        return Results.BadRequest("File is empty");

                    var command = new UploadImageCommand(file, productId);

                    var result =
                        await mediator.ExecuteCommandAsync<UploadImageCommand, ImageModel>(
                            command,
                            cancellationToken);

                    return Results.Ok(result);
                })
            .WithTags("Images")
            .DisableAntiforgery()
            .RequireAuthorization();

        app.MapDelete(ApiRoutes.Images.Delete,
                async (IMediator mediator, Guid imageId, CancellationToken cancellationToken) =>
                {
                    var command = new DeleteImageCommand(imageId);

                    await mediator.ExecuteCommandAsync(command, cancellationToken);

                    return Results.NoContent();
                })
            .WithTags("Images")
            .RequireAuthorization();

        app.MapGet(ApiRoutes.Images.GetAll,
                async (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var query = new GetImagesQuery();

                    var result =
                        await mediator.ExecuteQueryAsync<GetImagesQuery, ICollection<ImageModel>>(
                            query,
                            cancellationToken);

                    return Results.Ok(result);
                })
            .WithTags("Images")
            .RequireAuthorization();

        app.MapGet(ApiRoutes.Images.GetImagesByProductId,
                async (IMediator mediator, Guid productId, CancellationToken cancellationToken) =>
                {
                    var query = new GetImagesByProductIdQuery(productId);

                    var result =
                        await mediator.ExecuteQueryAsync<GetImagesByProductIdQuery, ICollection<ImageModel>>(
                            query,
                            cancellationToken);

                    return Results.Ok(result);
                })
            .WithTags("Images")
            .RequireAuthorization();

        app.MapGet(ApiRoutes.Images.GetById,
                async (IMediator mediator, Guid imageId, CancellationToken cancellationToken) =>
                {
                    var query = new GetImageByIdQuery(imageId);

                    var result =
                        await mediator.ExecuteQueryAsync<GetImageByIdQuery, ImageModel>(
                            query,
                            cancellationToken);

                    return Results.Ok(result);
                })
            .WithTags("Images")
            .RequireAuthorization();

        return app;
    }
}