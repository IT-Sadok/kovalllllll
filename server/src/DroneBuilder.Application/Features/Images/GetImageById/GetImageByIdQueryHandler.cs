using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Common.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
namespace DroneBuilder.Application.Features.Images.GetImageById;

public class GetImageByIdQueryHandler(IImageRepository imageRepository)
    : IQueryHandler<GetImageByIdQuery, ImageModel>
{
    public async Task<Result<ImageModel>> ExecuteAsync(GetImageByIdQuery query, CancellationToken cancellationToken)
    {
        Image? image = await imageRepository.GetImageByIdAsync(query.ImageId, cancellationToken);

        if (image == null)
        {
            return Result.Fail<ImageModel>(new NotFoundError($"Image with id {query.ImageId} not found."));
        }

        return Result.Ok(image.ToModel());
    }
}

public record GetImageByIdQuery(Guid ImageId);
