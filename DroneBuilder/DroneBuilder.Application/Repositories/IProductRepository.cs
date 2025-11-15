using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Repositories;

public interface IProductRepository
{
    Task AddProductAsync(Product product, CancellationToken cancellationToken = default);
    Task<Product?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ICollection<Product>> GetProductsAsync(CancellationToken cancellationToken = default);

    Task<Product?> GetPropertiesByProductIdAsync(Guid productId,
        CancellationToken cancellationToken = default);

    void RemoveProduct(Product product);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}