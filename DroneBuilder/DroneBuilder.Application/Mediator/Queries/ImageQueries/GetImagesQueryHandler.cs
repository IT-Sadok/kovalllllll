using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.ImageQueries;

public class GetImagesQueryHandler(IImageRepository imageRepository, IMapper mapper)
    : IQueryHandler<GetImagesQuery, ImagesResponseModel>
{
    public async Task<ImagesResponseModel> ExecuteAsync(GetImagesQuery query, CancellationToken cancellationToken)
    {
        var images = await imageRepository.GetImagesAsync(cancellationToken);
        

        return mapper.Map<ImagesResponseModel>(images);
    }
}

public record GetImagesQuery;