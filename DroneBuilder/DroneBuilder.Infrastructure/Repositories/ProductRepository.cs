using DroneBuilder.Application.Models;
using DroneBuilder.Application.Models.ProductModels;
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

    public async Task<PagedResult<Product>> GetFilteredPagedProductsAsync(PaginationParams pagination,
        ProductFilterModel filter,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Products
            .AsNoTracking()
            .Include(p => p.Images)
            .Include(p => p.Properties)!
            .ThenInclude(prop => prop.Values)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            var name = filter.Name.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(name));
        }

        if (filter.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            var category = filter.Category.Trim().ToLower();
            query = query.Where(p => p.Category.ToLower() == category);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Product>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<ICollection<Product>> GetProductsByIdsAsync(ICollection<Guid> productIds,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    public void RemoveProduct(Product product)
    {
        dbContext.Products.Remove(product);
    }


    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}