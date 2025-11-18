using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DroneBuilder.Infrastructure.Repositories;

public class ProductRepository(ApplicationDbContext dbContext) : IProductRepository
{
    public async Task AddProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        await dbContext.Products.AddAsync(product, cancellationToken);
    }

    public async Task<Product?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .Include(p => p.Images)
            .Include(p => p.Properties)!
            .ThenInclude(prop => prop.Values)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<ICollection<Product>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Include(p => p.Images)
            .Include(p => p.Properties)!
            .ThenInclude(prop => prop.Values)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetPropertiesByProductIdAsync(Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Include(p => p.Properties)!
            .ThenInclude(prop => prop.Values)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
    }

    public void RemoveProduct(Product product)
    {
        dbContext.Products.Remove(product);
    }

    public async Task<ICollection<Product>> GetByCategoryAsync(string category, CancellationToken cancellationToken)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Where(p => p.Category == category)
            .Include(p => p.Images)
            .Include(p => p.Properties)!
            .ThenInclude(prop => prop.Values)
            .ToListAsync(cancellationToken);
    }

    public async Task<ICollection<Product>> GetByPriceAsync(decimal? minPrice, decimal? maxPrice,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Products.AsNoTracking()
            .Include(p => p.Images)
            .Include(p => p.Properties)!
            .ThenInclude(prop => prop.Values)
            .AsQueryable();

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<ICollection<Product>> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Where(p => p.Name.Contains(name))
            .Include(p => p.Images)
            .Include(p => p.Properties)!
            .ThenInclude(prop => prop.Values)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}