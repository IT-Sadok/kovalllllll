using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;

namespace DroneBuilder.Application.Mediator.Commands.ImageCommands;

public class DeleteImageCommandHandler(IAzureStorageService azureStorageService, IImageRepository imageRepository)
    : ICommandHandler<DeleteImageCommand>
{
    public async Task ExecuteCommandAsync(DeleteImageCommand command, CancellationToken cancellationToken)
    {
        var existingImage = await imageRepository.GetImageByIdAsync(command.ImageId, cancellationToken);
        if (existingImage is null)
        {
            throw new NotFoundException($"Image with id {command.ImageId} not found.");
        }

        await azureStorageService.DeleteFileAsync(existingImage.Url);

        imageRepository.RemoveImage(existingImage);
        await imageRepository.SaveChangesAsync(cancellationToken);
    }
}

public record DeleteImageCommand(Guid ImageId);