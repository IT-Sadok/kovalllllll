using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Compatibility.DeleteCompatibilityRule;

public sealed class DeleteCompatibilityRuleCommandValidator
    : AbstractValidator<DeleteCompatibilityRuleCommand>
{
    public DeleteCompatibilityRuleCommandValidator()
    {
        RuleFor(command => command.RuleId).NotEmpty();
    }
}
