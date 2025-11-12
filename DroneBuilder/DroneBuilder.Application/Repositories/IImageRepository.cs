using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Repositories;

public interface IImageRepository
{
    Task AddImageAsync(Image image, CancellationToken cancellationToken = default);
    Task GetImageByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Image> GetImageAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Image>> GetImagesAsync(CancellationToken cancellationToken = default);
    Task RemoveImageAsync(Image image, CancellationToken cancellationToken = default);
    Task UpdateImageAsync(Image image, CancellationToken cancellationToken = default);
    Task<IEnumerable<Image>> GetImagesByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}