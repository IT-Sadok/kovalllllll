using DroneBuilder.Application.Common.Pagination;
using DroneBuilder.Application.Features.Products;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Common.Repositories;

public interface IProductRepository
{
    Task AddProductAsync(Product product, CancellationToken cancellationToken = default);
    Task<Product?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Product?> GetPropertiesByProductIdAsync(Guid productId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Product>> GetFilteredPagedProductsAsync(PaginationParams pagination,
        ProductFilterModel filter,
        CancellationToken cancellationToken = default);

    Task<Product?> GetDelistedProductByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<Product>> GetDelistedProductsAsync(PaginationParams pagination,
        CancellationToken cancellationToken = default);

    Task<ICollection<Product>> GetProductsByIdsAsync(ICollection<Guid> productIds,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> GetCategoriesAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
