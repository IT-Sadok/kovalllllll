namespace DroneBuilder.Application.Features.Imports.RaceDayQuads;

public record ShopifyProduct(
    long Id,
    string Title,
    string Handle,
    string? BodyHtml,
    string? Vendor,
    string? ProductType,
    List<string> Tags,
    List<ShopifyVariant> Variants,
    List<ShopifyImage> Images);

public record ShopifyVariant(long Id, string Title, string Price);

public record ShopifyImage(string Src);
