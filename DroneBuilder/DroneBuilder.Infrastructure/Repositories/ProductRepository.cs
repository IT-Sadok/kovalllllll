using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DroneBuilder.Infrastructure.Repositories;

public class ProductRepository(ApplicationDbContext dbContext) : IProductRepository
{
    public async Task AddProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        await dbContext.Products.AddAsync(product, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await dbContext.Products.FindAsync([id], cancellationToken: cancellationToken);

        if (product == null)
        {
            throw new NotFoundException($"Product with id {id} not found.");
        }
    }

    public async Task<Product> GetProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Products.FindAsync([id], cancellationToken: cancellationToken)
               ?? throw new NotFoundException($"Product with id {id} not found.");
    }

    public async Task<IEnumerable<Product>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Products.ToListAsync(cancellationToken);
    }

    public async Task RemoveProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        var existingProduct = await GetProductAsync(product.Id, cancellationToken);
        dbContext.Products.Remove(existingProduct);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        var existingProduct = await GetProductAsync(product.Id, cancellationToken);

        existingProduct.Name = product.Name;
        existingProduct.Price = product.Price;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}