using DroneBuilder.Domain.Entities.Components;

namespace DroneBuilder.Application.Features.Builds.Compatibility;

public static class SpecLabels
{
    public static string Of(MountPattern pattern) => pattern switch
    {
        MountPattern.M9x9 => "9x9 mm",
        MountPattern.M12x12 => "12x12 mm",
        MountPattern.M16x16 => "16x16 mm",
        MountPattern.M19x19 => "19x19 mm",
        MountPattern.M20x20 => "20x20 mm",
        MountPattern.M25_5x25_5 => "25.5x25.5 mm",
        MountPattern.M30_5x30_5 => "30.5x30.5 mm",
        _ => pattern.ToString()
    };

    public static string Of(IEnumerable<MountPattern> patterns) => string.Join(" or ", patterns.Select(Of));

    public static string Of(VideoSystem system) => system switch
    {
        VideoSystem.DjiO3 => "DJI O3",
        VideoSystem.DjiO4 => "DJI O4",
        VideoSystem.HdZero => "HDZero",
        _ => system.ToString()
    };

    public static string Of(BatteryConnector connector) => connector switch
    {
        BatteryConnector.Bt20 => "BT2.0",
        BatteryConnector.Ph20 => "PH2.0",
        _ => connector.ToString().ToUpperInvariant()
    };

    public static string Of(RfConnector connector) => connector switch
    {
        RfConnector.Ufl => "U.FL",
        RfConnector.RpSma => "RP-SMA",
        _ => connector.ToString().ToUpperInvariant()
    };

    public static string Of(RadioProtocol protocol) => protocol switch
    {
        RadioProtocol.ExpressLrs => "ExpressLRS",
        RadioProtocol.FrSky => "FrSky",
        _ => protocol.ToString()
    };

    public static string Cells(int min, int max) => min == max ? $"{min}S" : $"{min}-{max}S";
}
