using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Images.DeleteImage;

public class DeleteImageCommandHandler(IAzureStorageService azureStorageService, IImageRepository imageRepository)
    : ICommandHandler<DeleteImageCommand>
{
    public async Task<Result> ExecuteCommandAsync(DeleteImageCommand command, CancellationToken cancellationToken)
    {
        Image? existingImage = await imageRepository.GetImageByIdAsync(command.ImageId, cancellationToken);
        if (existingImage is null)
        {
            return Result.Fail(new NotFoundError($"Image with id {command.ImageId} not found."));
        }

        // If we are deleting the primary image, promote another image in the same database operation.
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

        // Database state is authoritative. A failed blob cleanup must not leave a dangling DB record.
        try
        {
            await azureStorageService.DeleteFileAsync(existingImage.Url, cancellationToken);
        }
        catch
        {
            // Blob cleanup can be retried operationally without breaking catalogue consistency.
        }

        return Result.Ok();
    }
}

public record DeleteImageCommand(Guid ImageId);
