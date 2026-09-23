using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;
namespace DroneBuilder.Application.Mediator.Queries.ImageQueries;

public class GetImagesQueryHandler(IImageRepository imageRepository)
    : IQueryHandler<GetImagesQuery, ICollection<ImageModel>>
{
    public async Task<Result<ICollection<ImageModel>>> ExecuteAsync(GetImagesQuery query, CancellationToken cancellationToken)
    {
        ICollection<Image> images = await imageRepository.GetImagesAsync(cancellationToken);

        return Result.Ok<ICollection<ImageModel>>(images.Select(x => x.ToModel()).ToList());
    }
}

public record GetImagesQuery;
