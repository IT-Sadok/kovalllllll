using DroneBuilder.Application.Features.Imports.RaceDayQuads;

namespace DroneBuilder.Application.Common.Abstractions;

public interface IRaceDayQuadsClient
{
    Task<IReadOnlyList<ShopifyProduct>> GetCollectionProductsAsync(string collectionHandle, int page, int limit,
        CancellationToken cancellationToken = default);
}
