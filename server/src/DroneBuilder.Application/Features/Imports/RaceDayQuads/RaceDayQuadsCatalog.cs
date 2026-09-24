using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Features.Imports.RaceDayQuads;

public record RaceDayQuadsCollection(string Handle, decimal? PropSizeInch = null);

public record RaceDayQuadsCategorySource(
    ProductCategory Category,
    IReadOnlyList<RaceDayQuadsCollection> Collections,
    IReadOnlyList<string> ProductTypes);

public static class RaceDayQuadsCatalog
{
    public const string Source = "racedayquads";

    public static readonly IReadOnlyList<string> ExcludedTitleWords =
        ["replacement", "spare", "frame parts", "parts kit", "top plate", "bottom plate", "arm set"];

    public static bool IsExcluded(ShopifyProduct product, RaceDayQuadsCategorySource source)
    {
        string productType = (product.ProductType ?? string.Empty).Trim().ToLowerInvariant();
        if (source.ProductTypes.Count > 0 && !source.ProductTypes.Contains(productType))
        {
            return true;
        }

        string title = product.Title.ToLowerInvariant();
        return ExcludedTitleWords.Any(title.Contains);
    }

    public static readonly IReadOnlyList<RaceDayQuadsCategorySource> Categories =
    [
        new(ProductCategory.Frame,
            [new("5-frames", 5m), new("3-frames", 3m), new("7-frames", 7m)],
            ["frame", "frames", "drone frames"]),
        new(ProductCategory.Motor,
            [new("22xx-brushless-motors"), new("23xx-brushless-motors"), new("28xx-brushless-motors")],
            ["motor", "drone motors"]),
        new(ProductCategory.Propeller,
            [new("5-props", 5m), new("3-props", 3m), new("7-props", 7m)],
            ["prop", "props", "drone props"]),
        new(ProductCategory.FlightController,
            [new("full-size-30x30-flight-controllers")],
            ["fc"]),
        new(ProductCategory.Esc,
            [new("4in1-escs")],
            ["esc"]),
        new(ProductCategory.Stack,
            [new("20x20-stacks")],
            ["stack", "drone fc+esc"]),
        new(ProductCategory.Battery,
            [new("6s-batteries"), new("4s-batteries")],
            ["battery", "batteries"]),
        new(ProductCategory.VideoTransmitter,
            [new("hd-vtxs"), new("30x30-stackable-video-transmitters")],
            ["vtx", "camera vtx"]),
        new(ProductCategory.Camera,
            [new("all-cameras")],
            ["camera"]),
        new(ProductCategory.Receiver,
            [new("all-receivers-1")],
            ["rc rx", "drone receivers"]),
        new(ProductCategory.Antenna,
            [new("vtx-receiver-antennas")],
            ["antenna", "rc antenna"]),
        new(ProductCategory.Goggles,
            [new("fpv-goggles")],
            ["goggle", "goggles"]),
        new(ProductCategory.Radio,
            [new("all-radio-controllers")],
            ["rc tx", "transmitter radio", "radio transmitters", "remote controllers"]),
        new(ProductCategory.Charger,
            [new("chargers-w-xt60")],
            ["charger"]),
        new(ProductCategory.ReadyToFly,
            [new("5-bnf-pnp-rtf-quadcopters-bind-n-flys-plug-n-plays-ready-to-flys", 5m)],
            [])
    ];
}
