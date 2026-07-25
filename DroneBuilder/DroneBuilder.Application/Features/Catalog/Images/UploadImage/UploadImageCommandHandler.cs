using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.ImageEvents;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Images.UploadImage;

public class UploadImageCommandHandler(
    IImageRepository imageRepository,
    IProductRepository productRepository,
    IAzureStorageService azureStorageService,
    IOutboxEventService outboxService,
    MessageQueuesConfiguration queuesConfig)
    : ICommandHandler<UploadImageCommand, ImageModel>
{
    public async Task<Result<ImageModel>> ExecuteCommandAsync(UploadImageCommand command,
        CancellationToken cancellationToken)
    {
        Product? product = await productRepository.GetProductByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Fail<ImageModel>(new NotFoundError(
                $"Product with id {command.ProductId} not found."));
        }

        (bool success, string? url) = await azureStorageService.UploadFileAsync(command.File, cancellationToken);

        if (!success)
        {
            return Result.Fail<ImageModel>(new ValidationError("Failed to upload image to storage."));
        }

        try
        {
            ICollection<Image> existingImages =
                await imageRepository.GetImagesByProductIdAsync(command.ProductId, cancellationToken);

            var image = new Image
            {
                ProductId = command.ProductId,
                Url = url,
                FileName = command.File.FileName,
                UploadedAt = DateTime.UtcNow,
                IsPrimary = existingImages.Count == 0
            };

            await imageRepository.AddImageAsync(image, cancellationToken);

            var @event = new ImageUploadedEvent(image.Id, command.ProductId);
            await outboxService.StoreEventAsync(@event, queuesConfig.ImageQueue.Name, cancellationToken);
            await imageRepository.SaveChangesAsync(cancellationToken);

            return Result.Ok(image.ToModel());
        }
        catch
        {
            try
            {
                await azureStorageService.DeleteFileAsync(url, CancellationToken.None);
            }
            catch
            {
                // Blob cleanup is best-effort; preserve the original persistence exception.
            }

            throw;
        }
    }
}

public record UploadImageCommand(FileUpload File, Guid ProductId);
