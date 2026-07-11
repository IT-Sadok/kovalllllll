using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Mediator.Commands.ImageCommands;

public class SetPrimaryImageCommandHandler(IImageRepository imageRepository)
    : ICommandHandler<SetPrimaryImageCommand>
{
    public async Task<Result> ExecuteCommandAsync(SetPrimaryImageCommand command, CancellationToken cancellationToken)
    {
        Image? targetImage = await imageRepository.GetImageByIdAsync(command.ImageId, cancellationToken);

        if (targetImage == null)
        {
            return Result.Fail(new ValidationError("Image not found."));
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
