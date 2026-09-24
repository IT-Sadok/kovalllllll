using DroneBuilder.Application.Features.Imports.RaceDayQuads;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Entities.Components;

namespace DroneBuilder.Application.Tests.Features.Imports;

public class RaceDayQuadsProductParserTests
{
    private const string BaseUrl = "https://www.racedayquads.com/";

    private static ShopifyProduct Product(string title, string body, List<string> tags, params ShopifyVariant[] variants)
        => new(123, title, "handle", body, "VENDOR", "Motor", tags,
            variants.Length > 0 ? variants.ToList() : [new ShopifyVariant(1, "Default Title", "19.99")],
            [new ShopifyImage("https://cdn.shopify.com/s/files/a.jpg?v=1")]);

    [Fact]
    public void Parse_WhenMotorHasKvVariants_ShouldCreateOneProductPerVariantWithOwnSpec()
    {
        // Arrange
        ShopifyProduct product = Product(
            "RDQ Badass 2 - 2207.5 Motor - 1400KV/1900KV",
            """
            <p>Specifications</p>
            <p>Stator Size: 2207.5<br>Motor Mounting Pattern: 16x16 mm<br>Weight: 37g (40g with hardware)</p>
            <p>1400KV</p><p>Rated Voltage (LiPo): 4-6S<br>Peak Current (60S): 33.3A<br>Max Pull: 1835g</p>
            <p>1900KV</p><p>Rated Voltage (LiPo): 4-6S<br>Peak Current (60S): 45.7A<br>Max Pull: 2058g</p>
            """,
            ["Manufacturer_RDQ", "Motor Bolt Pattern_16x16mm", "Shaft Size_5mm", "Stator Size_2207.5"],
            new ShopifyVariant(11, "1400Kv", "14.29"),
            new ShopifyVariant(12, "1900Kv", "14.29"));

        // Act
        IReadOnlyList<ImportedProduct> items =
            RaceDayQuadsProductParser.Parse(product, ProductCategory.Motor, new RaceDayQuadsCollection("22xx"), 6, BaseUrl);

        // Assert
        Assert.Equal(2, items.Count);
        ImportedProduct second = items[1];
        Assert.Equal("12", second.ExternalId);
        Assert.Equal("1900Kv", second.VariantName);
        Assert.Equal("RDQ Badass 2 - 2207.5 Motor - 1400KV/1900KV - 1900Kv", second.Name);
        Assert.Equal("RDQ", second.Manufacturer);
        Assert.Equal(37m, second.WeightGrams);
        Assert.Equal("https://www.racedayquads.com/products/handle?variant=12", second.SourceUrl);

        MotorSpec spec = Assert.IsType<MotorSpec>(second.Spec);
        Assert.Equal("2207.5", spec.StatorSize);
        Assert.Equal(1900, spec.Kv);
        Assert.Equal(MountPattern.M16x16, spec.MountPattern);
        Assert.Equal((4, 6), (spec.MinCells, spec.MaxCells));
        Assert.Equal(45.7m, spec.MaxCurrentA);
        Assert.Equal(5m, spec.ShaftMm);
        Assert.Equal(2058, spec.MaxThrustGrams);
        Assert.DoesNotContain(second.Attributes,
            a => a.Name is "Weight" or "Max Pull" or "Rated Voltage (LiPo)" or "Stator Size" or "Motor KV");
    }

    [Fact]
    public void Parse_WhenMotorCellsAreUnknown_ShouldLeaveSpecEmpty()
    {
        // Arrange
        ShopifyProduct product = Product("EMAX ECO II Series 2207 1900Kv Motor", "",
            ["Manufacturer_EMAX", "Motor Bolt Pattern_16x16mm", "Stator Size_2207"]);

        // Act
        ImportedProduct item = Assert.Single(
            RaceDayQuadsProductParser.Parse(product, ProductCategory.Motor, new RaceDayQuadsCollection("22xx"), 6, BaseUrl));

        // Assert
        Assert.Null(item.Spec);
        Assert.Null(item.VariantName);
        Assert.Equal("https://www.racedayquads.com/products/handle", item.SourceUrl);
    }

    [Fact]
    public void Parse_WhenBattery_ShouldReadCellsCapacityRatingAndConnector()
    {
        // Arrange
        ShopifyProduct product = Product("RDQ Series 22.2V 6S 6000mAh 100C LiPo Battery - XT90 Anti Spark",
            "<p>Cells: 6S<br>Capacity: 6000mAh<br>Discharge Rate: 100 C<br>Discharge Lead: XT90 (Anti Spark) 74 mm Long<br>Weight: 842 g</p>",
            ["Connector_XT90", "Input Voltage_6S", "Manufacturer_RDQ"]);

        // Act
        ImportedProduct item = Assert.Single(
            RaceDayQuadsProductParser.Parse(product, ProductCategory.Battery, new RaceDayQuadsCollection("6s"), 6, BaseUrl));

        // Assert
        BatterySpec spec = Assert.IsType<BatterySpec>(item.Spec);
        Assert.Equal(6, spec.Cells);
        Assert.Equal(6000, spec.CapacityMah);
        Assert.Equal(100, spec.CRating);
        Assert.Equal(BatteryConnector.Xt90, spec.Connector);
        Assert.Equal(842m, item.WeightGrams);
    }

    [Fact]
    public void Parse_WhenFrame_ShouldTakePropSizeFromCollectionAndMountsFromTags()
    {
        // Arrange
        ShopifyProduct product = Product("Lumenier QAV-S 2 JohnnyFPV SE 5\" Frame Kit",
            "<p>Wheelbase: 226mm<br>Weight: 133g</p>",
            ["Stack Size_20x20", "Stack Size_30x30", "Stack Size_Whoop", "Motor Bolt Pattern_16x16mm"]);

        // Act
        ImportedProduct item = Assert.Single(RaceDayQuadsProductParser.Parse(product, ProductCategory.Frame,
            new RaceDayQuadsCollection("5-frames", 5m), 6, BaseUrl));

        // Assert
        FrameSpec spec = Assert.IsType<FrameSpec>(item.Spec);
        Assert.Equal(5m, spec.MaxPropSizeInch);
        Assert.Equal([MountPattern.M20x20, MountPattern.M30_5x30_5], spec.FcMountPatterns);
        Assert.Equal([MountPattern.M16x16], spec.MotorMountPatterns);
        Assert.Null(spec.CameraWidthMm);
        Assert.Contains(item.Attributes, a => a is { Name: "Wheelbase", Value: "226mm" });
    }

    [Fact]
    public void Parse_WhenPropHasPitchTag_ShouldReadDiameterPitchBladesAndHub()
    {
        // Arrange
        ShopifyProduct product = Product("Gemfan Hurricane 51433 Durable Tri-Blade 5\" Prop 4 Pack", "",
            ["Prop Pitch_5035", "Shaft Size_5mm"]);

        // Act
        ImportedProduct item = Assert.Single(RaceDayQuadsProductParser.Parse(product, ProductCategory.Propeller,
            new RaceDayQuadsCollection("5-props", 5m), 6, BaseUrl));

        // Assert
        PropellerSpec spec = Assert.IsType<PropellerSpec>(item.Spec);
        Assert.Equal(5m, spec.DiameterInch);
        Assert.Equal(3.5m, spec.PitchInch);
        Assert.Equal(3, spec.BladeCount);
        Assert.Equal(5m, spec.HubMm);
    }

    [Fact]
    public void Parse_WhenNonComponentCategory_ShouldKeepAttributesWithoutSpec()
    {
        // Arrange
        ShopifyProduct product = Product("DJI Goggles 3", "<p>Screen: 1920x1080 Micro-OLED<br>Weight: 470g</p>", []);

        // Act
        ImportedProduct item = Assert.Single(RaceDayQuadsProductParser.Parse(product, ProductCategory.Goggles,
            new RaceDayQuadsCollection("fpv-goggles"), 6, BaseUrl));

        // Assert
        Assert.Null(item.Spec);
        Assert.Equal("VENDOR", item.Manufacturer);
        Assert.Equal(470m, item.WeightGrams);
        Assert.Contains(item.Attributes, a => a.Name == "Screen");
    }

    [Fact]
    public void Parse_ShouldCapVariants()
    {
        // Arrange
        ShopifyVariant[] variants = Enumerable.Range(1, 10)
            .Select(i => new ShopifyVariant(i, $"Color {i}", "4.99")).ToArray();
        ShopifyProduct product = Product("Prop 4 Pack", "", ["Prop Pitch_5035"], variants);

        // Act
        IReadOnlyList<ImportedProduct> items = RaceDayQuadsProductParser.Parse(product, ProductCategory.Propeller,
            new RaceDayQuadsCollection("5-props", 5m), 4, BaseUrl);

        // Assert
        Assert.Equal(4, items.Count);
    }

    [Theory]
    [InlineData("NewBeeDrone INERTIA 5\" FPV Frame Parts - Top Plate", "Frames", true)]
    [InlineData("PlasmaFPV Hyperion SE 5\" Racing Frame Kit", "Frame", false)]
    [InlineData("WREKD Frame Sticker", "WREKD Swag", true)]
    public void IsExcluded_ShouldDropPartsAndForeignTypes(string title, string productType, bool expected)
    {
        // Arrange
        RaceDayQuadsCategorySource frames = RaceDayQuadsCatalog.Categories.First(c => c.Category == ProductCategory.Frame);
        var product = new ShopifyProduct(1, title, "h", null, null, productType, [], [], []);

        // Act & Assert
        Assert.Equal(expected, RaceDayQuadsCatalog.IsExcluded(product, frames));
    }
}
