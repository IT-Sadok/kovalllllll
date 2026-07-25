using DroneBuilder.Application.Common;
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

    public Task<ProductCategory?> GetCategoryByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        string normalizedName = name.Trim().ToLower();
        string code = EntityCode.FromName(name);
        return dbContext.ProductCategories
            .FirstOrDefaultAsync(
                c => c.Name.ToLower() == normalizedName || c.Code == code,
                cancellationToken);
    }

    public async Task AddCategoryAsync(ProductCategory category, CancellationToken cancellationToken = default)
    {
        await dbContext.ProductCategories.AddAsync(category, cancellationToken);
    }

    public async Task<Product?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .AsSplitQuery()
            .Include(p => p.ProductCategory)
            .Include(p => p.ComponentType)
                .ThenInclude(type => type!.Properties)
            .Include(p => p.Variants)
                .ThenInclude(v => v.WarehouseItems)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Specifications)
            .Include(p => p.Images)
            .Include(p => p.ProductPropertyValues)
                .ThenInclude(ppv => ppv.Property)
            .Include(p => p.ProductPropertyValues)
                .ThenInclude(ppv => ppv.Value)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive, cancellationToken);
    }

    public async Task<ICollection<Product>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .AsNoTracking()
            .AsSplitQuery()
            .Where(p => p.IsActive)
            .Include(p => p.ProductCategory)
            .Include(p => p.ComponentType)
                .ThenInclude(type => type!.Properties)
            .Include(p => p.Variants)
                .ThenInclude(v => v.WarehouseItems)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Specifications)
            .Include(p => p.Images)
            .Include(p => p.ProductPropertyValues)
                .ThenInclude(ppv => ppv.Property)
            .Include(p => p.ProductPropertyValues)
                .ThenInclude(ppv => ppv.Value)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetPropertiesByProductIdAsync(Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.ProductCategory)
            .Include(p => p.ComponentType)
                .ThenInclude(type => type!.Properties)
            .Include(p => p.Variants)
                .ThenInclude(v => v.WarehouseItems)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Specifications)
            .Include(p => p.ProductPropertyValues)
                .ThenInclude(ppv => ppv.Property)
            .Include(p => p.ProductPropertyValues)
                .ThenInclude(ppv => ppv.Value)
            .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive, cancellationToken);
    }

    public async Task<PagedResult<Product>> GetFilteredPagedProductsAsync(PaginationParams pagination,
        ProductFilterModel filter,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = dbContext.Products
            .AsNoTracking()
            .AsSplitQuery()
            .Where(p => p.IsActive)
            .Include(p => p.ProductCategory)
            .Include(p => p.ComponentType)
                .ThenInclude(type => type!.Properties)
            .Include(p => p.Variants)
                .ThenInclude(v => v.WarehouseItems)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Specifications)
            .Include(p => p.Images)
            .Include(p => p.ProductPropertyValues)
                .ThenInclude(ppv => ppv.Property)
            .Include(p => p.ProductPropertyValues)
                .ThenInclude(ppv => ppv.Value)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            string name = filter.Name.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(name));
        }

        if (filter.MinPrice.HasValue)
        {
            query = query.Where(p => p.Variants.Any(v => v.IsDefault && v.Price >= filter.MinPrice.Value));
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Variants.Any(v => v.IsDefault && v.Price <= filter.MaxPrice.Value));
        }

        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            string category = filter.Category.Trim().ToLower();
            query = query.Where(p => p.ProductCategory!.Name.ToLower() == category);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        List<Product> items = await query
            .OrderBy(product => product.Name)
            .ThenBy(product => product.Id)
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
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .Include(p => p.ProductCategory)
            .Include(p => p.ComponentType)
                .ThenInclude(type => type!.Properties)
            .Include(p => p.Variants)
                .ThenInclude(v => v.WarehouseItems)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Specifications)
            .ToListAsync(cancellationToken);
    }

    public void RemoveProduct(Product product)
    {
        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        foreach (ProductVariant variant in product.Variants)
        {
            variant.IsActive = false;
            variant.UpdatedAt = DateTime.UtcNow;
        }
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.ProductCategories
            .Where(c => c.IsActive && c.Products.Any(p => p.IsActive))
            .OrderBy(c => c.Name)
            .Select(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
