using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.ImageQueries;

public class GetImagesQueryHandler(IImageRepository imageRepository, IMapper mapper)
    : IQueryHandler<GetImagesQuery, ICollection<ImageModel>>
{
    public async Task<ICollection<ImageModel>> ExecuteAsync(GetImagesQuery query, CancellationToken cancellationToken)
    {
        var images = await imageRepository.GetImagesAsync(cancellationToken);
        

        return mapper.Map<ICollection<ImageModel>>(images);
    }
}

public record GetImagesQuery;