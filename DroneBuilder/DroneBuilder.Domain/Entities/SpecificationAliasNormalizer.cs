using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace DroneBuilder.Domain.Entities;

public static partial class SpecificationAliasNormalizer
{
    public static string Normalize(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        string normalized = value
            .Normalize(NormalizationForm.FormKC)
            .Trim()
            .ToLowerInvariant();

        return WhitespaceRegex().Replace(normalized, " ");
    }

    public static bool TryParseDimensionPair(
        string value,
        out ParsedDimensionPair dimensions)
    {
        dimensions = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        Match match = DimensionPairRegex().Match(value.Trim());
        if (!match.Success ||
            !TryParseDecimal(match.Groups["width"].Value, out decimal width) ||
            !TryParseDecimal(match.Groups["height"].Value, out decimal height) ||
            width <= 0 ||
            height <= 0)
        {
            return false;
        }

        string firstUnitAlias = match.Groups["unit1"].Value;
        string secondUnitAlias = match.Groups["unit2"].Value;
        if (!string.IsNullOrWhiteSpace(firstUnitAlias) &&
            !string.IsNullOrWhiteSpace(secondUnitAlias) &&
            Normalize(firstUnitAlias) != Normalize(secondUnitAlias))
        {
            return false;
        }

        string unitAlias = !string.IsNullOrWhiteSpace(secondUnitAlias)
            ? secondUnitAlias
            : firstUnitAlias;
        if (string.IsNullOrWhiteSpace(unitAlias))
        {
            unitAlias = "mm";
        }

        dimensions = new ParsedDimensionPair(
            width,
            height,
            Normalize(unitAlias));
        return true;
    }

    private static bool TryParseDecimal(string value, out decimal result)
        => decimal.TryParse(
            value.Replace(',', '.'),
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out result);

    [GeneratedRegex(
        @"^\s*(?<width>\d+(?:[.,]\d+)?)\s*(?<unit1>mm|millimeters?|millimetres?|in|inch(?:es)?|[""″])?\s*[x×х]\s*(?<height>\d+(?:[.,]\d+)?)\s*(?<unit2>mm|millimeters?|millimetres?|in|inch(?:es)?|[""″])?\s*$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex DimensionPairRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}

public readonly record struct ParsedDimensionPair(
    decimal Width,
    decimal Height,
    string UnitAlias);
