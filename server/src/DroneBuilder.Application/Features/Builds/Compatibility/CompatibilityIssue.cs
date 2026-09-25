using System.Text.Json.Serialization;

namespace DroneBuilder.Application.Features.Builds.Compatibility;

[JsonConverter(typeof(JsonStringEnumConverter<IssueSeverity>))]
public enum IssueSeverity
{
    Error = 0,
    Warning = 1,
    Info = 2
}

public record CompatibilityIssue(IssueSeverity Severity, string Code, string Message, IReadOnlyList<Guid> ProductIds);
