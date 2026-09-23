using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Images.SetPrimaryImage;

public class SetPrimaryImageCommandHandler(IImageRepository imageRepository)
    : ICommandHandler<SetPrimaryImageCommand>
{
    public async Task<Result> ExecuteCommandAsync(SetPrimaryImageCommand command, CancellationToken cancellationToken)
    {
        Image? targetImage = await imageRepository.GetImageByIdAsync(command.ImageId, cancellationToken);

        if (targetImage == null)
        {
            return Result.Fail(new NotFoundError($"Image with id {command.ImageId} not found."));
        }

        ICollection<Image> productImages = await imageRepository.GetImagesByProductIdAsync(targetImage.ProductId, cancellationToken);

        foreach (Image img in productImages)
        {
            img.IsPrimary = img.Id == command.ImageId;
        }

        await imageRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}

public record SetPrimaryImageCommand(Guid ImageId);
