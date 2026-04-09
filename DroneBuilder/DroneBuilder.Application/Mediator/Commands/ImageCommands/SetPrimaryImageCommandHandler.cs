using System.ComponentModel.DataAnnotations;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;

namespace DroneBuilder.Application.Mediator.Commands.ImageCommands;

public class SetPrimaryImageCommandHandler(IImageRepository imageRepository)
    : ICommandHandler<SetPrimaryImageCommand>
{
    public async Task ExecuteCommandAsync(SetPrimaryImageCommand command, CancellationToken cancellationToken)
    {
        var targetImage = await imageRepository.GetImageByIdAsync(command.ImageId, cancellationToken);

        if (targetImage == null)
        {
            throw new ValidationException("Image not found.");
        }

        var productImages = await imageRepository.GetImagesByProductIdAsync(targetImage.ProductId, cancellationToken);

        foreach (var img in productImages)
        {
            img.IsPrimary = img.Id == command.ImageId;
        }

        await imageRepository.SaveChangesAsync(cancellationToken);
    }
}

public record SetPrimaryImageCommand(Guid ImageId);