using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DroneBuilder.Infrastructure.Repositories;

public class ImageRepository(ApplicationDbContext dbContext) : IImageRepository
{
    public async Task AddImageAsync(Image image, CancellationToken cancellationToken = default)
    {
        await dbContext.Images.AddAsync(image, cancellationToken);
    }

    public async Task<Image?> GetImageByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Images.FindAsync([id],
            cancellationToken: cancellationToken);
    }

    public async Task<ICollection<Image>> GetImagesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Images.ToListAsync(cancellationToken);
    }

    public async Task<ICollection<Image>> GetImagesByProductIdAsync(Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Images
            .Where(image => image.ProductId == productId)
            .ToListAsync(cancellationToken);
    }

    public void RemoveImage(Image image)
    {
        dbContext.Images.Remove(image);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}