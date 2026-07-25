using DroneBuilder.Application.Features.Catalog.Compatibility.Models;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Features.Catalog.Compatibility;

internal static class CompatibilityEvaluator
{
    public static CompatibilityCheckResultModel Evaluate(
        ProductVariant requestedLeft,
        ProductVariant requestedRight,
        ICollection<CompatibilityRule> rules)
    {
        var results = new List<CompatibilityRuleResultModel>();
        foreach (CompatibilityRule rule in rules)
        {
            bool requestMatchesRuleOrientation =
                requestedLeft.Product!.ComponentTypeId == rule.LeftComponentTypeId;
            ProductVariant left = requestMatchesRuleOrientation ? requestedLeft : requestedRight;
            ProductVariant right = requestMatchesRuleOrientation ? requestedRight : requestedLeft;
            RuleEvaluation evaluation = EvaluateRule(rule, left, right);
            results.Add(new CompatibilityRuleResultModel
            {
                RuleId = rule.Id,
                Code = rule.Code,
                Name = rule.Name,
                Status = evaluation.Status.ToString(),
                Message = evaluation.Message
            });
        }

        CompatibilityStatus status = results.Count == 0 || results.Any(result => result.Status == "Unknown")
            ? CompatibilityStatus.Unknown
            : results.Any(result => result.Status == "Incompatible")
                ? CompatibilityStatus.Incompatible
                : CompatibilityStatus.Compatible;
        if (results.Any(result => result.Status == "Incompatible"))
        {
            status = CompatibilityStatus.Incompatible;
        }

        return new CompatibilityCheckResultModel
        {
            Status = status.ToString(),
            IsCompatible = status == CompatibilityStatus.Unknown
                ? null
                : status == CompatibilityStatus.Compatible,
            LeftVariantId = requestedLeft.Id,
            RightVariantId = requestedRight.Id,
            Rules = results
        };
    }

    private static RuleEvaluation EvaluateRule(
        CompatibilityRule rule,
        ProductVariant leftVariant,
        ProductVariant rightVariant)
    {
        List<ComparableValue> leftValues = GetValues(leftVariant, rule.LeftPropertyId);
        List<ComparableValue> rightValues = GetValues(rightVariant, rule.RightPropertyId);
        if (leftValues.Count == 0 || rightValues.Count == 0)
        {
            return new RuleEvaluation(
                CompatibilityStatus.Unknown,
                "One or both components do not provide the required compatibility value.");
        }

        bool? compatible = rule.Operator switch
        {
            CompatibilityOperator.Equals => CompareEquals(leftValues, rightValues),
            CompatibilityOperator.AnyOptionMatches => CompareOptions(leftValues, rightValues),
            CompatibilityOperator.LeftLessThanOrEqualRight => CompareNumeric(
                leftValues, rightValues, (left, right) => left.Max <= right.Min),
            CompatibilityOperator.LeftGreaterThanOrEqualRight => CompareNumeric(
                leftValues, rightValues, (left, right) => left.Min >= right.Max),
            CompatibilityOperator.LeftContainedByRightRange => CompareNumeric(
                leftValues, rightValues, (left, right) => left.Min >= right.Min && left.Max <= right.Max),
            CompatibilityOperator.RightContainedByLeftRange => CompareNumeric(
                leftValues, rightValues, (left, right) => right.Min >= left.Min && right.Max <= left.Max),
            CompatibilityOperator.RangesOverlap => CompareNumeric(
                leftValues, rightValues, (left, right) => left.Min <= right.Max && right.Min <= left.Max),
            _ => null
        };

        return compatible switch
        {
            true => new RuleEvaluation(CompatibilityStatus.Compatible, "Compatibility rule passed."),
            false => new RuleEvaluation(
                CompatibilityStatus.Incompatible,
                rule.FailureMessage ?? $"Compatibility rule '{rule.Name}' failed."),
            null => new RuleEvaluation(
                CompatibilityStatus.Unknown,
                "The available values cannot be evaluated by this compatibility operator.")
        };
    }

    private static List<ComparableValue> GetValues(ProductVariant variant, Guid propertyId)
    {
        IEnumerable<ComparableValue> productValues = variant.Product!.ProductPropertyValues
            .Where(specification => specification.PropertyId == propertyId)
            .Select(specification => ComparableValue.From(
                specification.Property!,
                specification.Value,
                specification.TextValue,
                specification.NumericValue,
                specification.MinNumericValue,
                specification.MaxNumericValue,
                specification.BooleanValue));
        IEnumerable<ComparableValue> variantValues = variant.Specifications
            .Where(specification => specification.PropertyId == propertyId)
            .Select(specification => ComparableValue.From(
                specification.Property!,
                specification.Value,
                specification.TextValue,
                specification.NumericValue,
                specification.MinNumericValue,
                specification.MaxNumericValue,
                specification.BooleanValue));
        return productValues.Concat(variantValues).ToList();
    }

    private static bool? CompareEquals(
        ICollection<ComparableValue> leftValues,
        ICollection<ComparableValue> rightValues)
    {
        bool evaluated = false;
        foreach (ComparableValue left in leftValues)
        {
            foreach (ComparableValue right in rightValues)
            {
                if (left.NumericInterval.HasValue && right.NumericInterval.HasValue)
                {
                    evaluated = true;
                    if (left.NumericInterval.Value == right.NumericInterval.Value)
                    {
                        return true;
                    }
                }

                if (left.OptionCode is not null && right.OptionCode is not null)
                {
                    evaluated = true;
                    if (string.Equals(left.OptionCode, right.OptionCode, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                if (left.BooleanValue.HasValue && right.BooleanValue.HasValue)
                {
                    evaluated = true;
                    if (left.BooleanValue == right.BooleanValue)
                    {
                        return true;
                    }
                }

                if (left.NormalizedText is not null && right.NormalizedText is not null)
                {
                    evaluated = true;
                    if (left.NormalizedText == right.NormalizedText)
                    {
                        return true;
                    }
                }
            }
        }

        return evaluated ? false : null;
    }

    private static bool? CompareOptions(
        ICollection<ComparableValue> leftValues,
        ICollection<ComparableValue> rightValues)
    {
        string[] leftOptions = leftValues
            .Select(value => value.OptionCode)
            .Where(code => code is not null)
            .Cast<string>()
            .ToArray();
        string[] rightOptions = rightValues
            .Select(value => value.OptionCode)
            .Where(code => code is not null)
            .Cast<string>()
            .ToArray();
        if (leftOptions.Length == 0 || rightOptions.Length == 0)
        {
            return null;
        }

        return leftOptions.Intersect(rightOptions, StringComparer.OrdinalIgnoreCase).Any();
    }

    private static bool? CompareNumeric(
        ICollection<ComparableValue> leftValues,
        ICollection<ComparableValue> rightValues,
        Func<NumericInterval, NumericInterval, bool> predicate)
    {
        NumericInterval[] leftIntervals = leftValues
            .Where(value => value.NumericInterval.HasValue)
            .Select(value => value.NumericInterval!.Value)
            .ToArray();
        NumericInterval[] rightIntervals = rightValues
            .Where(value => value.NumericInterval.HasValue)
            .Select(value => value.NumericInterval!.Value)
            .ToArray();
        if (leftIntervals.Length == 0 || rightIntervals.Length == 0)
        {
            return null;
        }

        return leftIntervals.Any(left => rightIntervals.Any(right => predicate(left, right)));
    }

    private enum CompatibilityStatus
    {
        Compatible,
        Incompatible,
        Unknown
    }

    private readonly record struct RuleEvaluation(CompatibilityStatus Status, string Message);

    private readonly record struct NumericInterval(decimal Min, decimal Max);

    private sealed record ComparableValue(
        NumericInterval? NumericInterval,
        string? OptionCode,
        string? NormalizedText,
        bool? BooleanValue)
    {
        public static ComparableValue From(
            Property property,
            Value? option,
            string? textValue,
            decimal? numericValue,
            decimal? minNumericValue,
            decimal? maxNumericValue,
            bool? booleanValue)
        {
            decimal factor = property.UnitDefinition?.ConversionFactorToBase ?? 1m;
            NumericInterval? interval = numericValue.HasValue
                ? new NumericInterval(numericValue.Value * factor, numericValue.Value * factor)
                : minNumericValue.HasValue && maxNumericValue.HasValue
                    ? new NumericInterval(minNumericValue.Value * factor, maxNumericValue.Value * factor)
                    : option?.NumericValue is decimal optionNumber
                        ? new NumericInterval(optionNumber * factor, optionNumber * factor)
                        : null;
            return new ComparableValue(
                interval,
                option?.Code,
                textValue is null ? null : SpecificationAliasNormalizer.Normalize(textValue),
                booleanValue ?? option?.BooleanValue);
        }
    }
}
