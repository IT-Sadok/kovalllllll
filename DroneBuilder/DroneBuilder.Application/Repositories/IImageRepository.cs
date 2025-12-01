using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Repositories;

public interface IImageRepository
{
    Task AddImageAsync(Image image, CancellationToken cancellationToken = default);
    Task<Image?> GetImageByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ICollection<Image>> GetImagesAsync(CancellationToken cancellationToken = default);
    void RemoveImage(Image image);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}