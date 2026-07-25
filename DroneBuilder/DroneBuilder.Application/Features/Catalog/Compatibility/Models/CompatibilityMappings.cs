using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Features.Catalog.Compatibility.Models;

public static class CompatibilityMappings
{
    public static CompatibilityRuleModel ToModel(this CompatibilityRule rule) => new()
    {
        Id = rule.Id,
        Code = rule.Code,
        Name = rule.Name,
        LeftComponentTypeId = rule.LeftComponentTypeId,
        LeftPropertyId = rule.LeftPropertyId,
        RightComponentTypeId = rule.RightComponentTypeId,
        RightPropertyId = rule.RightPropertyId,
        Operator = rule.Operator.ToString(),
        FailureMessage = rule.FailureMessage,
        IsActive = rule.IsActive
    };
}
