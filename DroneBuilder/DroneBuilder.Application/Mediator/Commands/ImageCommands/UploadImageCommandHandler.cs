using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.ImageEvents;
using FluentResults;
using MapsterMapper;
using Microsoft.AspNetCore.Http;

namespace DroneBuilder.Application.Mediator.Commands.ImageCommands;

public class UploadImageCommandHandler(
    IImageRepository imageRepository,
    IAzureStorageService azureStorageService,
    IOutboxEventService outboxService,
    MessageQueuesConfiguration queuesConfig,
    IMapper mapper)
    : ICommandHandler<UploadImageCommand, ImageModel>
{
    public async Task<Result<ImageModel>> ExecuteCommandAsync(UploadImageCommand command,
        CancellationToken cancellationToken)
    {
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

        await imageRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok(mapper.Map<ImageModel>(image));
    }
}

public record UploadImageCommand(IFormFile File, Guid ProductId);
