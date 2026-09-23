using DroneBuilder.Application.Common.Abstractions;
using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Options;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.ImageEvents;
using FluentResults;
using Microsoft.AspNetCore.Http;
namespace DroneBuilder.Application.Features.Images.UploadImage;

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
            return Result.Fail<ImageModel>(new NotFoundError($"Product with id {command.ProductId} not found."));
        }

        (bool success, string? url) = await azureStorageService.UploadFileAsync(command.File, cancellationToken);

        if (!success)
        {
            return Result.Fail<ImageModel>(new ValidationError("Failed to upload image to storage."));
        }

        ICollection<Image> existingImages = await imageRepository.GetImagesByProductIdAsync(command.ProductId, cancellationToken);

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

        try
        {
            await imageRepository.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await azureStorageService.DeleteFileAsync(url, CancellationToken.None);
            throw;
        }

        return Result.Ok(image.ToModel());
    }
}

public record UploadImageCommand(IFormFile File, Guid ProductId);
