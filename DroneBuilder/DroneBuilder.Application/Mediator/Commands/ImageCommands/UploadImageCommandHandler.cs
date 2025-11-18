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
        var blobUrl = await azureStorageService.UploadFileAsync(command.File);

        var image = new Image
        {
            FileName = command.File.FileName,
            Url = blobUrl,
            UploadedAt = DateTime.UtcNow,
            ProductId = command.ProductId
        };

        await imageRepository.AddImageAsync(image, cancellationToken);
        await imageRepository.SaveChangesAsync(cancellationToken);

        return mapper.Map<ImageResponseModel>(image);
    }
}

public record UploadImageCommand(IFormFile File, Guid ProductId);