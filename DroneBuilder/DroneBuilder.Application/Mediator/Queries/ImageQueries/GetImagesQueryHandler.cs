using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.ImageQueries;

public class GetImagesQueryHandler(IImageRepository imageRepository, IMapper mapper)
    : IQueryHandler<GetImagesQuery, ICollection<ImageModel>>
{
    public async Task<Result<ICollection<ImageModel>>> ExecuteAsync(GetImagesQuery query, CancellationToken cancellationToken)
    {
        ICollection<Image> images = await imageRepository.GetImagesAsync(cancellationToken);

        return Result.Ok(mapper.Map<ICollection<ImageModel>>(images));
    }
}

public record GetImagesQuery;
