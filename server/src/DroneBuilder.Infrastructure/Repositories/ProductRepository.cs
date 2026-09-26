using DroneBuilder.Application.Common.Pagination;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Features.Products;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Entities.Components;
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
        ICollection<Guid>? onlyProductIds,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = dbContext.Products
            .AsNoTracking()
            .Where(p => !p.IsDeleted);

        if (onlyProductIds is not null)
        {
            query = query.Where(p => onlyProductIds.Contains(p.Id));
        }

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

        if (!string.IsNullOrWhiteSpace(filter.Manufacturer))
        {
            string manufacturer = filter.Manufacturer.Trim().ToLower();
            query = query.Where(p => p.Manufacturer != null && p.Manufacturer.ToLower() == manufacturer);
        }

        if (filter.InStock == true)
        {
            query = query.Where(p => dbContext.WarehouseItems.Any(w => w.ProductId == p.Id && w.Quantity > 0));
        }

        query = ApplySpecFilters(query, filter);

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

        IOrderedQueryable<Product> ordered = filter.Sort switch
        {
            ProductSort.PriceAsc => query.OrderBy(p => p.Price).ThenBy(p => p.Name),
            ProductSort.PriceDesc => query.OrderByDescending(p => p.Price).ThenBy(p => p.Name),
            _ => query.OrderBy(p => p.Name)
        };

        List<Product> items = await ordered
            .ThenBy(p => p.Id)
            .Include(p => p.Images)
            .Include(p => p.Spec)
            .Include(p => p.Attributes)
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

    private static IQueryable<Product> ApplySpecFilters(IQueryable<Product> query, ProductFilterModel filter)
    {
        if (filter.Cells is int cells)
        {
            query = query.Where(p =>
                (p.Spec is MotorSpec && ((MotorSpec)p.Spec).MinCells <= cells && ((MotorSpec)p.Spec).MaxCells >= cells) ||
                (p.Spec is FlightControllerSpec && ((FlightControllerSpec)p.Spec).MinCells <= cells &&
                 ((FlightControllerSpec)p.Spec).MaxCells >= cells) ||
                (p.Spec is EscSpec && ((EscSpec)p.Spec).MinCells <= cells && ((EscSpec)p.Spec).MaxCells >= cells) ||
                (p.Spec is StackSpec && ((StackSpec)p.Spec).MinCells <= cells && ((StackSpec)p.Spec).MaxCells >= cells) ||
                (p.Spec is BatterySpec && ((BatterySpec)p.Spec).Cells == cells));
        }

        if (filter.MountPattern is MountPattern mount)
        {
            query = query.Where(p =>
                (p.Spec is FrameSpec && (((FrameSpec)p.Spec).FcMountPatterns.Contains(mount) ||
                                         ((FrameSpec)p.Spec).MotorMountPatterns.Contains(mount))) ||
                (p.Spec is MotorSpec && ((MotorSpec)p.Spec).MountPattern == mount) ||
                (p.Spec is FlightControllerSpec && ((FlightControllerSpec)p.Spec).MountPattern == mount) ||
                (p.Spec is EscSpec && ((EscSpec)p.Spec).MountPattern == mount) ||
                (p.Spec is StackSpec && ((StackSpec)p.Spec).MountPattern == mount) ||
                (p.Spec is VideoTransmitterSpec && ((VideoTransmitterSpec)p.Spec).MountPattern == mount));
        }

        if (filter.VideoSystem is VideoSystem videoSystem)
        {
            query = query.Where(p =>
                (p.Spec is CameraSpec && ((CameraSpec)p.Spec).VideoSystem == videoSystem) ||
                (p.Spec is VideoTransmitterSpec && ((VideoTransmitterSpec)p.Spec).VideoSystem == videoSystem));
        }

        if (filter.KvMin.HasValue || filter.KvMax.HasValue)
        {
            int kvMin = filter.KvMin ?? 0;
            int kvMax = filter.KvMax ?? int.MaxValue;
            query = query.Where(p =>
                p.Spec is MotorSpec && ((MotorSpec)p.Spec).Kv >= kvMin && ((MotorSpec)p.Spec).Kv <= kvMax);
        }

        if (filter.PropSizeInch is decimal propSize)
        {
            decimal nextSize = propSize + 1;
            query = query.Where(p =>
                (p.Spec is PropellerSpec && ((PropellerSpec)p.Spec).DiameterInch >= propSize &&
                 ((PropellerSpec)p.Spec).DiameterInch < nextSize) ||
                (p.Spec is FrameSpec && ((FrameSpec)p.Spec).MaxPropSizeInch >= propSize &&
                 ((FrameSpec)p.Spec).MaxPropSizeInch < nextSize));
        }

        if (filter.CapacityMin.HasValue || filter.CapacityMax.HasValue)
        {
            int capacityMin = filter.CapacityMin ?? 0;
            int capacityMax = filter.CapacityMax ?? int.MaxValue;
            query = query.Where(p => p.Spec is BatterySpec && ((BatterySpec)p.Spec).CapacityMah >= capacityMin &&
                                     ((BatterySpec)p.Spec).CapacityMah <= capacityMax);
        }

        if (filter.BatteryConnector is BatteryConnector batteryConnector)
        {
            query = query.Where(p =>
                (p.Spec is BatterySpec && ((BatterySpec)p.Spec).Connector == batteryConnector) ||
                (p.Spec is EscSpec && ((EscSpec)p.Spec).BatteryConnector == batteryConnector) ||
                (p.Spec is StackSpec && ((StackSpec)p.Spec).BatteryConnector == batteryConnector));
        }

        if (filter.Protocol is RadioProtocol protocol)
        {
            query = query.Where(p => p.Spec is ReceiverSpec && ((ReceiverSpec)p.Spec).Protocol == protocol);
        }

        if (filter.RfConnector is RfConnector rfConnector)
        {
            query = query.Where(p =>
                (p.Spec is AntennaSpec && ((AntennaSpec)p.Spec).Connector == rfConnector) ||
                (p.Spec is VideoTransmitterSpec && ((VideoTransmitterSpec)p.Spec).AntennaConnector == rfConnector));
        }

        return query;
    }

    public async Task<ICollection<string>> GetManufacturersAsync(ProductCategory? category,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.Manufacturer != null && p.Manufacturer != "")
            .Where(p => category == null || p.Category == category)
            .Select(p => p.Manufacturer!)
            .Distinct()
            .OrderBy(m => m)
            .ToListAsync(cancellationToken);
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

    public async Task<ICollection<Product>> GetCategoryProductsWithSpecsAsync(ProductCategory category,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Include(p => p.Spec)
            .Where(p => p.Category == category && !p.IsDeleted)
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
