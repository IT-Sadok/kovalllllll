using DroneBuilder.Application.Features.Imports.RaceDayQuads;
using DroneBuilder.Domain.Entities.Components;

namespace DroneBuilder.Application.Tests.Features.Imports;

public class SpecTextTests
{
    [Theory]
    [InlineData("16x16mm", MountPattern.M16x16)]
    [InlineData("20 x 20mm", MountPattern.M20x20)]
    [InlineData("30.5x30.5", MountPattern.M30_5x30_5)]
    [InlineData("25.5×25.5 mm", MountPattern.M25_5x25_5)]
    [InlineData("M2 9x9", MountPattern.M9x9)]
    public void MountPatterns_WhenPatternPresent_ShouldParse(string text, MountPattern expected)
    {
        // Act
        List<MountPattern> patterns = SpecText.MountPatterns(text);

        // Assert
        Assert.Equal([expected], patterns);
    }

    [Fact]
    public void MountPatterns_WhenSeveralPatterns_ShouldReturnAllDistinct()
    {
        // Act
        List<MountPattern> patterns = SpecText.MountPatterns("VTX Mounting Pattern: 20x20/25.5x25.5mm, 20x20");

        // Assert
        Assert.Equal([MountPattern.M20x20, MountPattern.M25_5x25_5], patterns);
    }

    [Theory]
    [InlineData(new[] { "3S", "4S", "6S" }, 3, 6)]
    [InlineData(new[] { "Rated Voltage (LiPo): 4-6S" }, 4, 6)]
    [InlineData(new[] { "2S-8S" }, 2, 8)]
    [InlineData(new[] { "6S (12-16V)" }, 6, 6)]
    [InlineData(new[] { "3S~6S" }, 3, 6)]
    public void CellRange_ShouldReadSinglesAndRanges(string[] values, int min, int max)
    {
        // Act
        (int Min, int Max)? range = SpecText.CellRange(values);

        // Assert
        Assert.Equal((min, max), range);
    }

    [Fact]
    public void CellRange_WhenOnlyVoltsGiven_ShouldReturnNull()
    {
        // Act & Assert
        Assert.Null(SpecText.CellRange(["5V", "7.4-26.4 V"]));
    }

    [Theory]
    [InlineData("37g (40g with hardware)", 37)]
    [InlineData(".96g", 0.96)]
    [InlineData("About 50g (frame only)", 50)]
    [InlineData("Approx. 1.2 kg", 1200)]
    public void Grams_ShouldReadFirstMass(string text, decimal expected)
    {
        // Act & Assert
        Assert.Equal(expected, SpecText.Grams(text));
    }

    [Theory]
    [InlineData("XT90 (Anti Spark) 74 mm Long", BatteryConnector.Xt90)]
    [InlineData("xt-60", BatteryConnector.Xt60)]
    [InlineData("XT150", BatteryConnector.Xt150)]
    [InlineData("BT2.0", BatteryConnector.Bt20)]
    public void ParseBatteryConnector_ShouldRecognizeCommonPlugs(string text, BatteryConnector expected)
    {
        // Act & Assert
        Assert.Equal(expected, SpecText.ParseBatteryConnector(text));
    }

    [Theory]
    [InlineData("IPEX 1", RfConnector.Ufl)]
    [InlineData("U.FL", RfConnector.Ufl)]
    [InlineData("IPEX 4", RfConnector.Ipex4)]
    [InlineData("RP-SMA", RfConnector.RpSma)]
    [InlineData("SMA 90", RfConnector.Sma)]
    [InlineData("MMCX 90", RfConnector.Mmcx)]
    [InlineData("MCX Male", RfConnector.Mcx)]
    public void ParseRfConnector_ShouldRecognizeCommonConnectors(string text, RfConnector expected)
    {
        // Act & Assert
        Assert.Equal(expected, SpecText.ParseRfConnector(text));
    }

    [Theory]
    [InlineData("DJI O4 Air Unit Pro", false, VideoSystem.DjiO4)]
    [InlineData("Walksnail Avatar HD Pro Kit", true, VideoSystem.Walksnail)]
    [InlineData("HDZero Freestyle V2 VTX", true, VideoSystem.HdZero)]
    [InlineData("RunCam Phoenix 2 Nano", false, VideoSystem.Analog)]
    [InlineData("Caddx Nebula Pro Vista Kit", true, null)]
    public void DetectVideoSystem_ShouldNotGuessUnknownHdSystems(string text, bool taggedAsHd, VideoSystem? expected)
    {
        // Act & Assert
        Assert.Equal(expected, SpecText.DetectVideoSystem(text, taggedAsHd));
    }

    [Fact]
    public void ExtractKeyValues_WhenBodyHasPerVariantSections_ShouldPreferOwnSection()
    {
        // Arrange
        const string html = """
            <p>Specifications</p>
            <p>Stator Size: 2207.5<br>Motor KV: 1400KV, 1900KV</p>
            <p>1400KV</p><p>Max Pull: 1835g<br>Peak Current (60S): 33.3A</p>
            <p>1900KV</p><p>Max Pull: 2058g<br>Peak Current (60S): 45.7A</p>
            """;

        // Act
        List<KeyValuePair<string, string>> values = SpecText.ExtractKeyValues(html, "1900Kv", ["1400Kv", "1900Kv"]);

        // Assert
        Assert.Contains(values, p => p is { Key: "Stator Size", Value: "2207.5" });
        Assert.Contains(values, p => p is { Key: "Max Pull", Value: "2058g" });
        Assert.DoesNotContain(values, p => p.Value == "1835g");
    }
}
