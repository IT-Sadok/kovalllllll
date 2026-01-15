using System.ComponentModel.DataAnnotations;
using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.ImageEvents;
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
    public async Task<ImageModel> ExecuteCommandAsync(UploadImageCommand command,
        CancellationToken cancellationToken)
    {
        var (success, url) = await azureStorageService.UploadFileAsync(command.File, cancellationToken);

        if (!success)
        {
            throw new ValidationException("Failed to upload image to storage.");
        }

        var image = new Image
        {
            ProductId = command.ProductId,
            Url = url,
            FileName = command.File.FileName,
            UploadedAt = DateTime.UtcNow
        };

        await imageRepository.AddImageAsync(image, cancellationToken);

        var @event = new ImageUploadedEvent(image.Id, command.ProductId);
        await outboxService.StoreEventAsync(@event, queuesConfig.ImageQueue.Name, cancellationToken);

        await imageRepository.SaveChangesAsync(cancellationToken);

        return mapper.Map<ImageModel>(image);
    }
}

public record UploadImageCommand(IFormFile File, Guid ProductId);