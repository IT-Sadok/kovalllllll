using System.Net.Http.Json;
using System.Text.Json;
using DroneBuilder.Application.Common.Abstractions;
using DroneBuilder.Application.Common.Options;
using DroneBuilder.Application.Features.Imports.RaceDayQuads;

namespace DroneBuilder.Infrastructure.Imports;

public class RaceDayQuadsClient(HttpClient httpClient, RaceDayQuadsImportOptions options) : IRaceDayQuadsClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    public async Task<IReadOnlyList<ShopifyProduct>> GetCollectionProductsAsync(string collectionHandle, int page,
        int limit, CancellationToken cancellationToken = default)
    {
        await Task.Delay(options.RequestDelayMs, cancellationToken);

        string url = $"collections/{Uri.EscapeDataString(collectionHandle)}/products.json?limit={limit}&page={page}";
        ShopifyProductsResponse? response =
            await httpClient.GetFromJsonAsync<ShopifyProductsResponse>(url, JsonOptions, cancellationToken);

        return response?.Products ?? [];
    }

    private sealed record ShopifyProductsResponse(List<ShopifyProduct> Products);
}
