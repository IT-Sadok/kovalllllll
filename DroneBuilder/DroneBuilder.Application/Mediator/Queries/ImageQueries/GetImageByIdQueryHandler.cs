using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.ImageQueries;

public class GetImageByIdQueryHandler(IImageRepository imageRepository, IMapper mapper)
    : IQueryHandler<GetImageByIdQuery, ImageModel>
{
    public async Task<Result<ImageModel>> ExecuteAsync(GetImageByIdQuery query, CancellationToken cancellationToken)
    {
        Image? image = await imageRepository.GetImageByIdAsync(query.ImageId, cancellationToken);

        if (image == null)
        {
            return Result.Fail<ImageModel>(new NotFoundError($"Image with id {query.ImageId} not found."));
        }

        return Result.Ok(mapper.Map<ImageModel>(image));
    }
}

public record GetImageByIdQuery(Guid ImageId);
