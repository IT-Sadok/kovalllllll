using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.ImageQueries;

public class GetImageByIdQueryHandler(IImageRepository imageRepository, IMapper mapper)
    : IQueryHandler<GetImageByIdQuery, ImageModel>
{
    public async Task<ImageModel> ExecuteAsync(GetImageByIdQuery query, CancellationToken cancellationToken)
    {
        Image? image = await imageRepository.GetImageByIdAsync(query.ImageId, cancellationToken);

        if (image == null)
        {
            throw new NotFoundException($"Image with id {query.ImageId} not found.");
        }

        return mapper.Map<ImageModel>(image);
    }
}

public record GetImageByIdQuery(Guid ImageId);
