using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Repositories;

public interface ICatalogImportRepository
{
    Task<ICollection<ImportSource>> GetSourcesAsync(CancellationToken cancellationToken = default);
    Task<ImportSource?> GetSourceAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> IsSourceCodeInUseAsync(string code, Guid? excludedId = null, CancellationToken cancellationToken = default);
    Task AddSourceAsync(ImportSource source, CancellationToken cancellationToken = default);

    Task<ICollection<ImportBatch>> GetBatchesAsync(Guid sourceId, CancellationToken cancellationToken = default);
    Task<ImportBatch?> GetBatchAsync(Guid sourceId, Guid batchId, CancellationToken cancellationToken = default);

    Task<Product?> GetProductAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Property?> GetPropertyAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Value?> GetValueAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductExternalReference?> GetProductReferenceAsync(Guid sourceId, string externalId, CancellationToken cancellationToken = default);
    Task<ProductVariantExternalReference?> GetVariantReferenceAsync(Guid sourceId, string externalId, CancellationToken cancellationToken = default);
    Task AddProductReferenceAsync(ProductExternalReference reference, CancellationToken cancellationToken = default);
    Task AddVariantReferenceAsync(ProductVariantExternalReference reference, CancellationToken cancellationToken = default);
    void RemoveProductReference(ProductExternalReference reference);
    void RemoveVariantReference(ProductVariantExternalReference reference);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
