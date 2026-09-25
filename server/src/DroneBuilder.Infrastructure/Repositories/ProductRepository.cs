using DroneBuilder.Application.Common.Pagination;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Features.Products;
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
            .Include(p => p.Spec)
            .Include(p => p.Attributes)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
    }

    public async Task<ICollection<Product>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .Include(p => p.Images)
            .Include(p => p.Attributes)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<Product>> GetFilteredPagedProductsAsync(PaginationParams pagination,
        ProductFilterModel filter,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = dbContext.Products
            .AsNoTracking()
            .Where(p => !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            string name = filter.Name.Trim().ToLower();
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

        if (filter.Category.HasValue)
        {
            query = query.Where(p => p.Category == filter.Category.Value);
        }

        if (filter.CollapseVariants == true)
        {
            IQueryable<Product> matching = query;
            query = query.Where(p => p.GroupId == null || p.Id == matching
                .Where(v => v.GroupId == p.GroupId)
                .OrderBy(v => v.Price)
                .ThenBy(v => v.Name)
                .Select(v => v.Id)
                .First());
        }

        int totalCount = await query.CountAsync(cancellationToken);

        List<Product> items = await query
            .Include(p => p.Images)
            .Include(p => p.Spec)
            .Include(p => p.Attributes)
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id)
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

    public async Task<Product?> GetDelistedProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted, cancellationToken);
    }

    public async Task<PagedResult<Product>> GetDelistedProductsAsync(PaginationParams pagination,
        CancellationToken cancellationToken = default)
    {
        IOrderedQueryable<Product> query = dbContext.Products
            .AsNoTracking()
            .Where(p => p.IsDeleted)
            .Include(p => p.Images)
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id);

        int totalCount = await query.CountAsync(cancellationToken);

        List<Product> items = await query
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
            .Where(p => productIds.Contains(p.Id) && !p.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<ICollection<Product>> GetProductsWithSpecsByIdsAsync(ICollection<Guid> productIds,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Include(p => p.Spec)
            .Where(p => productIds.Contains(p.Id) && !p.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<ICollection<Product>> GetByExternalIdsAsync(string source, ICollection<string> externalIds,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .Include(p => p.Images)
            .Include(p => p.Spec)
            .Include(p => p.Attributes)
            .Where(p => p.ExternalSource == source && p.ExternalId != null && externalIds.Contains(p.ExternalId))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductGroup?> GetGroupByExternalIdAsync(string source, string externalId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.ProductGroups
            .FirstOrDefaultAsync(g => g.ExternalSource == source && g.ExternalId == externalId, cancellationToken);
    }

    public async Task AddGroupAsync(ProductGroup group, CancellationToken cancellationToken = default)
    {
        await dbContext.ProductGroups.AddAsync(group, cancellationToken);
    }

    public async Task<ICollection<ProductGroupSummary>> GetGroupSummariesAsync(ICollection<Guid> groupIds,
        CancellationToken cancellationToken = default)
    {
        return await (
                from product in dbContext.Products
                where product.GroupId != null && groupIds.Contains(product.GroupId.Value) && !product.IsDeleted
                join stock in dbContext.WarehouseItems on product.Id equals stock.ProductId into stocks
                from stock in stocks.DefaultIfEmpty()
                group new { product.Price, Quantity = stock == null ? 0 : stock.Quantity }
                    by new { GroupId = product.GroupId!.Value, product.Group!.Name } into g
                select new ProductGroupSummary(g.Key.GroupId, g.Key.Name, g.Count(), g.Min(x => x.Price),
                    g.Max(x => x.Price), g.Sum(x => x.Quantity)))
            .ToListAsync(cancellationToken);
    }

    public async Task<ICollection<Product>> GetGroupVariantsAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Where(p => p.GroupId == groupId && !p.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
