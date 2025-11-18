using System.ComponentModel.DataAnnotations;
using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using MapsterMapper;
using Microsoft.AspNetCore.Http;

namespace DroneBuilder.Application.Mediator.Commands.ImageCommands;

public class UploadImageCommandHandler(
    IImageRepository imageRepository,
    IAzureStorageService azureStorageService,
    IMapper mapper)
    : ICommandHandler<UploadImageCommand, ImageResponseModel>
{
    public async Task<ImageResponseModel> ExecuteCommandAsync(UploadImageCommand command,
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
        await imageRepository.SaveChangesAsync(cancellationToken);

        return mapper.Map<ImageResponseModel>(image);
    }
}

public record UploadImageCommand(IFormFile File, Guid ProductId);