namespace DroneBuilder.Application.Features.Catalog.Compatibility.Models;

public sealed class CompatibilityRuleModel
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public Guid LeftComponentTypeId { get; init; }
    public Guid LeftPropertyId { get; init; }
    public Guid RightComponentTypeId { get; init; }
    public Guid RightPropertyId { get; init; }
    public string Operator { get; init; } = string.Empty;
    public string? FailureMessage { get; init; }
    public bool IsActive { get; init; }
}

public sealed class CreateCompatibilityRuleModel
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public Guid LeftComponentTypeId { get; init; }
    public Guid LeftPropertyId { get; init; }
    public Guid RightComponentTypeId { get; init; }
    public Guid RightPropertyId { get; init; }
    public string Operator { get; init; } = string.Empty;
    public string? FailureMessage { get; init; }
}

public sealed class UpdateCompatibilityRuleModel
{
    public string? Code { get; init; }
    public string? Name { get; init; }
    public string? Operator { get; init; }
    public string? FailureMessage { get; init; }
    public bool ClearFailureMessage { get; init; }
    public bool? IsActive { get; init; }
}

public sealed class CheckCompatibilityModel
{
    public Guid LeftVariantId { get; init; }
    public Guid RightVariantId { get; init; }
}

public sealed class CompatibilityCheckResultModel
{
    public string Status { get; init; } = string.Empty;
    public bool? IsCompatible { get; init; }
    public Guid LeftVariantId { get; init; }
    public Guid RightVariantId { get; init; }
    public ICollection<CompatibilityRuleResultModel> Rules { get; init; } = [];
}

public sealed class CompatibilityRuleResultModel
{
    public Guid RuleId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
