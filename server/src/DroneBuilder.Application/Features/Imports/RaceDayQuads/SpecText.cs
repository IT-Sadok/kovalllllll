using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using DroneBuilder.Domain.Entities.Components;

namespace DroneBuilder.Application.Features.Imports.RaceDayQuads;

public static partial class SpecText
{
    public static IReadOnlyList<string> ToLines(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return [];
        }

        string text = BlockTagRegex().Replace(html, "\n");
        text = TagRegex().Replace(text, " ");
        text = WebUtility.HtmlDecode(text).Replace('\u00a0', ' ');

        return text.Split('\n')
            .Select(l => SpacesRegex().Replace(l, " ").Trim())
            .Where(l => l.Length > 0)
            .ToList();
    }

    public static string NormalizeKey(string key)
        => new(key.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());

    public static List<KeyValuePair<string, string>> ExtractKeyValues(string? html, string variantTitle,
        IReadOnlyCollection<string> allVariantTitles)
    {
        HashSet<string> sectionHeaders = allVariantTitles
            .Select(NormalizeKey)
            .Where(t => t.Length > 0 && t != "defaulttitle")
            .ToHashSet();
        string ownSection = NormalizeKey(variantTitle);

        var general = new List<KeyValuePair<string, string>>();
        var own = new List<KeyValuePair<string, string>>();
        string? currentSection = null;

        foreach (string line in ToLines(html))
        {
            Match match = KeyValueRegex().Match(line);
            if (!match.Success)
            {
                string normalized = NormalizeKey(line);
                if (sectionHeaders.Contains(normalized))
                {
                    currentSection = normalized;
                }

                continue;
            }

            var pair = new KeyValuePair<string, string>(match.Groups[1].Value.Trim(), match.Groups[2].Value.Trim());
            if (currentSection is null)
            {
                general.Add(pair);
            }
            else if (currentSection == ownSection)
            {
                own.Add(pair);
            }
        }

        HashSet<string> ownKeys = own.Select(p => NormalizeKey(p.Key)).ToHashSet();
        return general.Where(p => !ownKeys.Contains(NormalizeKey(p.Key))).Concat(own)
            .GroupBy(p => NormalizeKey(p.Key))
            .Select(g => g.First())
            .ToList();
    }

    public static decimal? FirstNumber(string? text)
    {
        if (text is null)
        {
            return null;
        }

        Match match = NumberRegex().Match(text);
        return match.Success ? decimal.Parse(match.Value, CultureInfo.InvariantCulture) : null;
    }

    public static decimal? Grams(string? text)
    {
        if (text is null)
        {
            return null;
        }

        Match match = MassRegex().Match(text);
        if (!match.Success)
        {
            return null;
        }

        decimal value = decimal.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        return match.Groups[2].Value.Equals("kg", StringComparison.OrdinalIgnoreCase) ? value * 1000 : value;
    }

    public static List<MountPattern> MountPatterns(string? text)
    {
        var patterns = new List<MountPattern>();
        if (text is null)
        {
            return patterns;
        }

        foreach (Match match in MountRegex().Matches(text))
        {
            decimal size = decimal.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            MountPattern? pattern = size switch
            {
                9 => MountPattern.M9x9,
                12 => MountPattern.M12x12,
                16 => MountPattern.M16x16,
                19 => MountPattern.M19x19,
                20 => MountPattern.M20x20,
                >= 25 and <= 26 => MountPattern.M25_5x25_5,
                >= 30 and <= 31 => MountPattern.M30_5x30_5,
                _ => null
            };

            if (pattern.HasValue && !patterns.Contains(pattern.Value))
            {
                patterns.Add(pattern.Value);
            }
        }

        return patterns;
    }

    public static (int Min, int Max)? CellRange(IEnumerable<string> values)
    {
        var cells = new List<int>();
        foreach (string value in values)
        {
            foreach (Match range in CellRangeRegex().Matches(value))
            {
                cells.Add(int.Parse(range.Groups[1].Value, CultureInfo.InvariantCulture));
                cells.Add(int.Parse(range.Groups[2].Value, CultureInfo.InvariantCulture));
            }

            foreach (Match single in CellRegex().Matches(value))
            {
                cells.Add(int.Parse(single.Groups[1].Value, CultureInfo.InvariantCulture));
            }
        }

        cells.RemoveAll(c => c is < 1 or > 12);
        return cells.Count == 0 ? null : (cells.Min(), cells.Max());
    }

    public static BatteryConnector? ParseBatteryConnector(string? text)
    {
        string compact = Compact(text);
        return compact switch
        {
            _ when compact.Contains("XT150") => BatteryConnector.Xt150,
            _ when compact.Contains("XT90") => BatteryConnector.Xt90,
            _ when compact.Contains("XT60") => BatteryConnector.Xt60,
            _ when compact.Contains("XT30") => BatteryConnector.Xt30,
            _ when compact.Contains("EC5") => BatteryConnector.Ec5,
            _ when compact.Contains("BT20") => BatteryConnector.Bt20,
            _ when compact.Contains("PH20") => BatteryConnector.Ph20,
            _ => null
        };
    }

    public static RfConnector? ParseRfConnector(string? text)
    {
        string compact = Compact(text);
        return compact switch
        {
            _ when compact.Contains("RPSMA") => RfConnector.RpSma,
            _ when compact.Contains("MMCX") => RfConnector.Mmcx,
            _ when compact.Contains("IPEX4") || compact.Contains("MHF4") => RfConnector.Ipex4,
            _ when compact.Contains("UFL") || compact.Contains("IPEX") || compact.Contains("IPX") => RfConnector.Ufl,
            _ when compact.Contains("MCX") => RfConnector.Mcx,
            _ when compact.Contains("SMA") => RfConnector.Sma,
            _ => null
        };
    }

    public static VideoSystem? DetectVideoSystem(string text, bool taggedAsHd)
    {
        string compact = Compact(text);
        if (compact.Contains("WALKSNAIL") || compact.Contains("AVATAR"))
        {
            return VideoSystem.Walksnail;
        }

        if (compact.Contains("HDZERO"))
        {
            return VideoSystem.HdZero;
        }

        if (compact.Contains("DJI"))
        {
            if (compact.Contains("O4"))
            {
                return VideoSystem.DjiO4;
            }

            return compact.Contains("O3") ? VideoSystem.DjiO3 : null;
        }

        return taggedAsHd ? null : VideoSystem.Analog;
    }

    public static RadioProtocol? DetectRadioProtocol(string text)
    {
        string compact = Compact(text);
        return compact switch
        {
            _ when compact.Contains("ELRS") || compact.Contains("EXPRESSLRS") => RadioProtocol.ExpressLrs,
            _ when compact.Contains("CROSSFIRE") || compact.Contains("CRSF") => RadioProtocol.Crossfire,
            _ when compact.Contains("TRACER") => RadioProtocol.Tracer,
            _ when compact.Contains("GHOST") => RadioProtocol.Ghost,
            _ when compact.Contains("FRSKY") || compact.Contains("ACCST") || compact.Contains("ARCHER") => RadioProtocol.FrSky,
            _ => null
        };
    }

    private static string Compact(string? text)
        => text is null ? string.Empty : new string(text.ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());

    [GeneratedRegex(@"<(br|/p|/li|/tr|/div|/h\d)[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex BlockTagRegex();

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex TagRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpacesRegex();

    [GeneratedRegex(@"^([A-Za-z][A-Za-z0-9 ()/.+\-&']{1,45}?)\s*[:：]\s*(.{1,300})$")]
    private static partial Regex KeyValueRegex();

    [GeneratedRegex(@"\d*\.?\d+")]
    private static partial Regex NumberRegex();

    [GeneratedRegex(@"(\d*\.?\d+)\s*(kg|g)\b", RegexOptions.IgnoreCase)]
    private static partial Regex MassRegex();

    [GeneratedRegex(@"(\d{1,2}(?:\.\d)?)\s*(?:mm)?\s*[x×*]\s*\d{1,2}(?:\.\d)?", RegexOptions.IgnoreCase)]
    private static partial Regex MountRegex();

    [GeneratedRegex(@"(\d{1,2})\s*S?\s*[-~–]\s*(\d{1,2})\s*S\b", RegexOptions.IgnoreCase)]
    private static partial Regex CellRangeRegex();

    [GeneratedRegex(@"(?<![\d.\-~–])(\d{1,2})\s*S\b(?!\s*[-~–]\s*\d)", RegexOptions.IgnoreCase)]
    private static partial Regex CellRegex();
}
