using System.Globalization;
using System.Text.RegularExpressions;
using DroneBuilder.Application.Features.Products;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Entities.Components;

namespace DroneBuilder.Application.Features.Imports.RaceDayQuads;

public record ImportedProduct(
    string ExternalId,
    string Name,
    string? VariantName,
    decimal Price,
    string? Manufacturer,
    decimal? WeightGrams,
    string SourceUrl,
    List<ProductAttributeModel> Attributes,
    ComponentSpec? Spec,
    List<string> ImageUrls);

public static partial class RaceDayQuadsProductParser
{
    private const string DefaultVariantTitle = "Default Title";
    private const int MaxNameLength = 200;
    private const int MaxAttributes = 50;
    private const int MaxImages = 5;

    public static IReadOnlyList<ImportedProduct> Parse(ShopifyProduct product, ProductCategory category,
        RaceDayQuadsCollection collection, int maxVariants, string baseUrl)
    {
        List<ShopifyVariant> variants = product.Variants.Take(maxVariants).ToList();
        List<string> variantTitles = product.Variants.Select(v => v.Title).ToList();
        List<string> imageUrls = product.Images.Take(MaxImages).Select(i => i.Src).ToList();
        string productUrl = $"{baseUrl.TrimEnd('/')}/products/{product.Handle}";

        return variants.Select(variant =>
        {
            string? variantName = variant.Title == DefaultVariantTitle ? null : variant.Title.Trim();
            var context = new ParseContext(product, variantName,
                SpecText.ExtractKeyValues(product.BodyHtml, variant.Title, variantTitles), collection);

            ComponentSpec? spec = BuildSpec(category, context);
            decimal? weight = SpecText.Grams(context.Value("weight", "netweight"));

            string name = variantName is null ? product.Title : $"{product.Title} - {variantName}";

            return new ImportedProduct(
                ExternalId: variant.Id.ToString(CultureInfo.InvariantCulture),
                Name: Truncate(name.Trim(), MaxNameLength),
                VariantName: variantName is null ? null : Truncate(variantName, 100),
                Price: decimal.Parse(variant.Price, CultureInfo.InvariantCulture),
                Manufacturer: context.Tag("Manufacturer").FirstOrDefault() ?? product.Vendor,
                WeightGrams: weight is > 0 and < 100000 ? weight : null,
                SourceUrl: variants.Count > 1 ? $"{productUrl}?variant={variant.Id}" : productUrl,
                Attributes: context.UnusedAttributes(MaxAttributes),
                Spec: spec,
                ImageUrls: imageUrls);
        }).ToList();
    }

    private static ComponentSpec? BuildSpec(ProductCategory category, ParseContext c) => category switch
    {
        ProductCategory.Frame => BuildFrame(c),
        ProductCategory.Motor => BuildMotor(c),
        ProductCategory.Propeller => BuildPropeller(c),
        ProductCategory.FlightController => BuildFlightController(c),
        ProductCategory.Esc => BuildEsc(c),
        ProductCategory.Stack => BuildStack(c),
        ProductCategory.Battery => BuildBattery(c),
        ProductCategory.VideoTransmitter => BuildVideoTransmitter(c),
        ProductCategory.Camera => BuildCamera(c),
        ProductCategory.Receiver => BuildReceiver(c),
        ProductCategory.Antenna => BuildAntenna(c),
        _ => null
    };

    private static FrameSpec? BuildFrame(ParseContext c)
    {
        decimal? propSize = c.Collection.PropSizeInch ?? InchSize(c.Product.Title);
        List<MountPattern> fcMounts = c.Tag("Stack Size").SelectMany(SpecText.MountPatterns)
            .Concat(SpecText.MountPatterns(c.Value("mountingsupport", "fcmount", "stackmount", "electronicsmount",
                "mountingpattern", "mountinghole")))
            .Distinct().ToList();
        List<MountPattern> motorMounts = c.Tag("Motor Bolt Pattern").SelectMany(SpecText.MountPatterns)
            .Concat(SpecText.MountPatterns(c.Value("motormount", "motorbolt", "motorhole")))
            .Distinct().ToList();

        if (propSize is null || fcMounts.Count == 0)
        {
            return null;
        }

        return new FrameSpec
        {
            MaxPropSizeInch = propSize.Value,
            FcMountPatterns = fcMounts,
            MotorMountPatterns = motorMounts,
            CameraWidthMm = CameraWidth(c.Value("camera") ?? c.Product.Title)
        };
    }

    private static MotorSpec? BuildMotor(ParseContext c)
    {
        string? shaftText = c.Value("shaftdimension", "shaftdiameter", "shaft");
        string? statorText = c.Value("statorsize");
        string? kvText = c.Value("motorkv", "kv");
        string? stator = SingleOrNull(c.Tag("Stator Size")) ?? Stator(statorText) ?? Stator(c.Product.Title);
        int? kv = Kv(c.VariantName) ?? SingleKv(kvText) ?? SingleKv(c.Product.Title);
        MountPattern? mount = c.Tag("Motor Bolt Pattern").SelectMany(SpecText.MountPatterns)
            .Concat(SpecText.MountPatterns(c.Value("motormountingpattern", "mountingpattern", "mountinghole", "motormount")))
            .Cast<MountPattern?>().FirstOrDefault();
        (int Min, int Max)? cells = Cells(c, "ratedvoltage", "inputvoltage", "voltage", "cells", "noofcells", "lipocells", "cellcount");

        if (stator is null || kv is null || mount is null || cells is null)
        {
            return null;
        }

        return new MotorSpec
        {
            StatorSize = stator,
            Kv = kv.Value,
            MountPattern = mount.Value,
            MinCells = cells.Value.Min,
            MaxCells = cells.Value.Max,
            MaxCurrentA = SpecText.FirstNumber(c.Value("peakcurrent", "maxcurrent", "maximumcurrent")),
            ShaftMm = SpecText.FirstNumber(SingleOrNull(c.Tag("Shaft Size")) ?? shaftText),
            MaxThrustGrams = (int?)SpecText.Grams(c.Value("maxpull", "maxthrust", "maximumthrust"))
        };
    }

    private static PropellerSpec? BuildPropeller(ParseContext c)
    {
        decimal? diameter = null;
        decimal? pitch = null;
        int? blades = null;

        Match size = PropSizeRegex().Match(c.Product.Title);
        if (size.Success)
        {
            diameter = decimal.Parse(size.Groups[1].Value, CultureInfo.InvariantCulture);
            pitch = decimal.Parse(size.Groups[2].Value, CultureInfo.InvariantCulture);
            blades = size.Groups[3].Success ? int.Parse(size.Groups[3].Value, CultureInfo.InvariantCulture) : null;
        }

        string? pitchTag = c.Tag("Prop Pitch").FirstOrDefault(t => PitchTagRegex().IsMatch(t));
        if (diameter is null && pitchTag is not null)
        {
            diameter = decimal.Parse(pitchTag[..2], CultureInfo.InvariantCulture) / 10;
            pitch = decimal.Parse(pitchTag[2..], CultureInfo.InvariantCulture) / 10;
        }

        diameter ??= c.Collection.PropSizeInch;
        blades ??= BladeCount(c.Product.Title);

        if (diameter is null)
        {
            return null;
        }

        return new PropellerSpec
        {
            DiameterInch = diameter.Value,
            PitchInch = pitch,
            BladeCount = blades,
            HubMm = SpecText.FirstNumber(SingleOrNull(c.Tag("Shaft Size")))
        };
    }

    private static FlightControllerSpec? BuildFlightController(ParseContext c)
    {
        MountPattern? mount = StackMount(c);
        (int Min, int Max)? cells = Cells(c, "inputvoltage", "voltage", "power");
        return mount is null || cells is null
            ? null
            : new FlightControllerSpec { MountPattern = mount.Value, MinCells = cells.Value.Min, MaxCells = cells.Value.Max };
    }

    private static EscSpec? BuildEsc(ParseContext c)
    {
        MountPattern? mount = StackMount(c);
        (int Min, int Max)? cells = Cells(c, "inputvoltage", "voltage", "lipo");
        if (mount is null || cells is null)
        {
            return null;
        }

        return new EscSpec
        {
            MountPattern = mount.Value,
            MinCells = cells.Value.Min,
            MaxCells = cells.Value.Max,
            ContinuousCurrentA = ContinuousCurrent(c),
            BatteryConnector = BatteryConnectorOf(c)
        };
    }

    private static StackSpec? BuildStack(ParseContext c)
    {
        MountPattern? mount = StackMount(c);
        (int Min, int Max)? cells = Cells(c, "inputvoltage", "voltage", "lipo");
        if (mount is null || cells is null)
        {
            return null;
        }

        return new StackSpec
        {
            MountPattern = mount.Value,
            MinCells = cells.Value.Min,
            MaxCells = cells.Value.Max,
            ContinuousCurrentA = ContinuousCurrent(c),
            BatteryConnector = BatteryConnectorOf(c)
        };
    }

    private static BatterySpec? BuildBattery(ParseContext c)
    {
        (int Min, int Max)? tagCells = SpecText.CellRange(c.Tag("Input Voltage"));
        int? cells = tagCells is { } t && t.Min == t.Max ? t.Min : null;
        cells ??= SpecText.CellRange([c.Value("cells", "cellcount", "configuration") ?? string.Empty, c.Product.Title]) switch
        {
            { } r when r.Min == r.Max => r.Min,
            _ => null
        };

        int? capacity = (int?)SpecText.FirstNumber(c.Value("capacity"))
            ?? CapacityRegex().Match(c.Product.Title) switch
            {
                { Success: true } m => int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture),
                _ => null
            };

        BatteryConnector? connector = SingleOrNull(c.Tag("Connector").Select(SpecText.ParseBatteryConnector)
                .Where(x => x is not null).Distinct())
            ?? SpecText.ParseBatteryConnector(c.Value("dischargelead", "connector", "plug"))
            ?? SpecText.ParseBatteryConnector(c.Product.Title);

        if (cells is null || capacity is null or <= 0 || connector is null)
        {
            return null;
        }

        return new BatterySpec
        {
            Cells = cells.Value,
            CapacityMah = capacity.Value,
            CRating = (int?)SpecText.FirstNumber(c.Value("dischargerate", "crating", "continuousdischarge"))
                ?? CRatingRegex().Match(c.Product.Title) switch
                {
                    { Success: true } m => int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture),
                    _ => null
                },
            Connector = connector.Value
        };
    }

    private static VideoTransmitterSpec? BuildVideoTransmitter(ParseContext c)
    {
        VideoSystem? system = VideoSystemOf(c);
        return system is null
            ? null
            : new VideoTransmitterSpec
            {
                VideoSystem = system.Value,
                AntennaConnector = c.Tag("Connector").Select(SpecText.ParseRfConnector).FirstOrDefault(x => x is not null),
                MountPattern = StackMount(c)
            };
    }

    private static CameraSpec? BuildCamera(ParseContext c)
    {
        VideoSystem? system = VideoSystemOf(c);
        return system is null
            ? null
            : new CameraSpec { VideoSystem = system.Value, WidthMm = CameraWidth(c.Product.Title) };
    }

    private static ReceiverSpec? BuildReceiver(ParseContext c)
    {
        RadioProtocol? protocol = SpecText.DetectRadioProtocol($"{c.Product.Title} {string.Join(' ', c.Product.Tags)}");
        return protocol is null ? null : new ReceiverSpec { Protocol = protocol.Value };
    }

    private static AntennaSpec? BuildAntenna(ParseContext c)
    {
        RfConnector? connector = c.Tag("Connector").Select(SpecText.ParseRfConnector).FirstOrDefault(x => x is not null)
            ?? SpecText.ParseRfConnector(c.VariantName)
            ?? SpecText.ParseRfConnector(c.Product.Title);
        return connector is null ? null : new AntennaSpec { Connector = connector.Value };
    }

    private static MountPattern? StackMount(ParseContext c)
        => c.Tag("Stack Size").SelectMany(SpecText.MountPatterns)
            .Concat(SpecText.MountPatterns(c.Value("mountinghole", "mountingpattern", "mounting")))
            .Cast<MountPattern?>().FirstOrDefault();

    private static (int Min, int Max)? Cells(ParseContext c, params string[] keys)
    {
        string? text = c.Value(keys);
        return SpecText.CellRange(c.Tag("Input Voltage")) ?? SpecText.CellRange([text ?? string.Empty]);
    }

    private static decimal? ContinuousCurrent(ParseContext c)
    {
        string? text = c.Value("continuouscurrent", "current");
        return SpecText.FirstNumber(SingleOrNull(c.Tag("Amp Rating")) ?? text);
    }

    private static BatteryConnector? BatteryConnectorOf(ParseContext c)
        => SpecText.ParseBatteryConnector(c.Value("batteryconnector", "powerlead", "batterylead"));

    private static VideoSystem? VideoSystemOf(ParseContext c)
        => SpecText.DetectVideoSystem($"{c.Product.Title} {c.Product.Vendor} {string.Join(' ', c.Tag("Manufacturer"))}",
            c.Tag("HD").Any());

    private static string? Stator(string? text)
        => text is not null && StatorRegex().Match(text) is { Success: true } m ? m.Groups[1].Value : null;

    private static int? Kv(string? text)
        => text is not null && KvRegex().Match(text) is { Success: true } m
            ? int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture)
            : null;

    private static int? SingleKv(string? text)
    {
        if (text is null)
        {
            return null;
        }

        List<int> values = KvRegex().Matches(text)
            .Select(m => int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture))
            .Distinct().ToList();
        return values.Count == 1 ? values[0] : null;
    }

    private static decimal? InchSize(string text)
        => InchRegex().Match(text) is { Success: true } m ? decimal.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) : null;

    private static int? CameraWidth(string? text)
    {
        string lower = text?.ToLowerInvariant() ?? string.Empty;
        return lower switch
        {
            _ when lower.Contains("nano") => 14,
            _ when lower.Contains("micro") => 19,
            _ when lower.Contains("mini") => 21,
            _ when lower.Contains("full size") => 28,
            _ => null
        };
    }

    private static int? BladeCount(string title)
    {
        string lower = title.ToLowerInvariant();
        return lower switch
        {
            _ when lower.Contains("tri-blade") || lower.Contains("triblade") || lower.Contains("3 blade") || lower.Contains("3-blade") => 3,
            _ when lower.Contains("bi-blade") || lower.Contains("2 blade") || lower.Contains("2-blade") => 2,
            _ when lower.Contains("quad-blade") || lower.Contains("4 blade") || lower.Contains("4-blade") => 4,
            _ => null
        };
    }

    private static T? SingleOrNull<T>(IEnumerable<T> values)
    {
        List<T> list = values.Distinct().ToList();
        return list.Count == 1 ? list[0] : default;
    }

    private static string Truncate(string value, int length) => value.Length <= length ? value : value[..length].TrimEnd();

    [GeneratedRegex(@"(\d{3,5})\s*kv\b", RegexOptions.IgnoreCase)]
    private static partial Regex KvRegex();

    [GeneratedRegex(@"(?<![\d.])(\d{4}(?:\.\d)?)(?![\d.])(?!\s*kv)", RegexOptions.IgnoreCase)]
    private static partial Regex StatorRegex();

    [GeneratedRegex(@"(\d(?:\.\d)?)\s*[x×]\s*(\d(?:\.\d+)?)(?:\s*[x×]\s*(\d))?", RegexOptions.IgnoreCase)]
    private static partial Regex PropSizeRegex();

    [GeneratedRegex(@"^\d{4}$")]
    private static partial Regex PitchTagRegex();

    [GeneratedRegex(@"(\d(?:\.\d)?)\s*(?:""|”|″|-?inch\b|in\b)", RegexOptions.IgnoreCase)]
    private static partial Regex InchRegex();

    [GeneratedRegex(@"(\d{3,5})\s*mAh", RegexOptions.IgnoreCase)]
    private static partial Regex CapacityRegex();

    [GeneratedRegex(@"\b(\d{2,3})\s*C\b")]
    private static partial Regex CRatingRegex();

    private sealed class ParseContext(
        ShopifyProduct product,
        string? variantName,
        List<KeyValuePair<string, string>> values,
        RaceDayQuadsCollection collection)
    {
        private readonly HashSet<string> _consumed = [];

        public ShopifyProduct Product => product;
        public string? VariantName => variantName;
        public RaceDayQuadsCollection Collection => collection;

        public IEnumerable<string> Tag(string key)
            => product.Tags
                .Where(t => t.StartsWith(key + "_", StringComparison.OrdinalIgnoreCase))
                .Select(t => t[(key.Length + 1)..].Trim())
                .Where(v => v.Length > 0);

        public string? Value(params string[] keys)
        {
            foreach (string key in keys)
            {
                KeyValuePair<string, string> match = values.FirstOrDefault(p => SpecText.NormalizeKey(p.Key).StartsWith(key));
                if (match.Key is not null)
                {
                    _consumed.Add(SpecText.NormalizeKey(match.Key));
                    return match.Value;
                }
            }

            return null;
        }

        public List<ProductAttributeModel> UnusedAttributes(int max)
            => values
                .Where(p => !_consumed.Contains(SpecText.NormalizeKey(p.Key)))
                .Take(max)
                .Select(p => new ProductAttributeModel(Truncate(p.Key, 100), Truncate(p.Value, 500)))
                .ToList();
    }
}
