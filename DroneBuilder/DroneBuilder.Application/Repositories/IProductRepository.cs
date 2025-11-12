using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Repositories;

public interface IProductRepository
{
    Task AddProductAsync(Product product, CancellationToken cancellationToken = default);
    Task GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Product> GetProductAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetProductsAsync(CancellationToken cancellationToken = default);
    Task RemoveProductAsync(Product product, CancellationToken cancellationToken = default);
    Task UpdateProductAsync(Product product, CancellationToken cancellationToken = default);
    Task GetProductImagesAsync(Product product, CancellationToken cancellationToken = default);
    Task GetProductPropertiesAsync(Product product, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}