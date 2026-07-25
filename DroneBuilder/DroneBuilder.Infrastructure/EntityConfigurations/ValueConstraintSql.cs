namespace DroneBuilder.Infrastructure.EntityConfigurations;

internal static class ValueConstraintSql
{
    public const string ExactlyOneRepresentation =
        "(CASE WHEN \"ValueId\" IS NOT NULL THEN 1 ELSE 0 END + " +
        "CASE WHEN \"TextValue\" IS NOT NULL THEN 1 ELSE 0 END + " +
        "CASE WHEN \"NumericValue\" IS NOT NULL THEN 1 ELSE 0 END + " +
        "CASE WHEN \"MinNumericValue\" IS NOT NULL OR \"MaxNumericValue\" IS NOT NULL THEN 1 ELSE 0 END + " +
        "CASE WHEN \"BooleanValue\" IS NOT NULL THEN 1 ELSE 0 END) = 1";

    public const string CompleteRange =
        "(\"MinNumericValue\" IS NULL AND \"MaxNumericValue\" IS NULL) OR " +
        "(\"MinNumericValue\" IS NOT NULL AND \"MaxNumericValue\" IS NOT NULL)";

    public const string ValidRange =
        "\"MinNumericValue\" IS NULL OR \"MaxNumericValue\" IS NULL OR " +
        "\"MinNumericValue\" <= \"MaxNumericValue\"";
}
