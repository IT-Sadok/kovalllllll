using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DroneBuilder.Infrastructure.Repositories;

public class ImageRepository(ApplicationDbContext dbContext) : IImageRepository
{
    public async Task AddImageAsync(Image image, CancellationToken cancellationToken = default)
    {
        await dbContext.Images.AddAsync(image, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task GetImageByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var image = await dbContext.Images.FindAsync([id],
            cancellationToken: cancellationToken);
        if (image == null)
        {
            throw new NotFoundException($"Image with id {id} not found.");
        }
    }

    public async Task<Image> GetImageAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Images.FindAsync([id],
            cancellationToken: cancellationToken) ?? throw new NotFoundException($"Image with id {id} not found.");
    }

    public async Task<IEnumerable<Image>> GetImagesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Images.ToListAsync(cancellationToken);
    }

    public async Task RemoveImageAsync(Image image, CancellationToken cancellationToken = default)
    {
        var existingImage = await GetImageAsync(image.Id, cancellationToken);
        dbContext.Images.Remove(existingImage);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateImageAsync(Image image, CancellationToken cancellationToken = default)
    {
        var existingImage = await GetImageAsync(image.Id, cancellationToken);

        existingImage.Url = image.Url;
        existingImage.FileName = image.FileName;
        existingImage.ProductId = image.ProductId;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Image>> GetImagesByProductIdAsync(Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Images
            .Where(image => image.ProductId == productId)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}