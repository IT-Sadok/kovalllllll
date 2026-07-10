using System.ComponentModel.DataAnnotations;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Mediator.Commands.ImageCommands;

public class SetPrimaryImageCommandHandler(IImageRepository imageRepository)
    : ICommandHandler<SetPrimaryImageCommand>
{
    public async Task ExecuteCommandAsync(SetPrimaryImageCommand command, CancellationToken cancellationToken)
    {
        Image? targetImage = await imageRepository.GetImageByIdAsync(command.ImageId, cancellationToken);

        if (targetImage == null)
        {
            throw new ValidationException("Image not found.");
        }

        ICollection<Image> productImages = await imageRepository.GetImagesByProductIdAsync(targetImage.ProductId, cancellationToken);

        foreach (Image img in productImages)
        {
            img.IsPrimary = img.Id == command.ImageId;
        }

        await imageRepository.SaveChangesAsync(cancellationToken);
    }
}

public record SetPrimaryImageCommand(Guid ImageId);
