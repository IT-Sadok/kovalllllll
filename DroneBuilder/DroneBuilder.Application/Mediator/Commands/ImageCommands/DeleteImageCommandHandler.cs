using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Mediator.Commands.ImageCommands;

public class DeleteImageCommandHandler(IAzureStorageService azureStorageService, IImageRepository imageRepository)
    : ICommandHandler<DeleteImageCommand>
{
    public async Task ExecuteCommandAsync(DeleteImageCommand command, CancellationToken cancellationToken)
    {
        Image? existingImage = await imageRepository.GetImageByIdAsync(command.ImageId, cancellationToken);
        if (existingImage is null)
        {
            throw new NotFoundException($"Image with id {command.ImageId} not found.");
        }

        await azureStorageService.DeleteFileAsync(existingImage.Url, cancellationToken);

        // If we are deleting the primary image, we should try to promote another one
        if (existingImage.IsPrimary)
        {
            ICollection<Image> otherImages = await imageRepository.GetImagesByProductIdAsync(existingImage.ProductId, cancellationToken);
            Image? nextPrimary = otherImages.FirstOrDefault(x => x.Id != existingImage.Id);
            if (nextPrimary != null)
            {
                nextPrimary.IsPrimary = true;
                // No need to call Update specifically if tracking is enabled, but ensuring it's in the repo context
            }
        }

        imageRepository.RemoveImage(existingImage);
        await imageRepository.SaveChangesAsync(cancellationToken);
    }
}

public record DeleteImageCommand(Guid ImageId);
