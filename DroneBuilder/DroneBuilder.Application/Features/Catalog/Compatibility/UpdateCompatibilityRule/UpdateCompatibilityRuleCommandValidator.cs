using DroneBuilder.Domain.Entities;
using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Compatibility.UpdateCompatibilityRule;

public sealed class UpdateCompatibilityRuleCommandValidator
    : AbstractValidator<UpdateCompatibilityRuleCommand>
{
    public UpdateCompatibilityRuleCommandValidator()
    {
        RuleFor(command => command.RuleId).NotEmpty();
        RuleFor(command => command.Model).NotNull();
        RuleFor(command => command.Model)
            .Must(model => model.Code is not null || model.Name is not null || model.Operator is not null ||
                           model.FailureMessage is not null || model.ClearFailureMessage || model.IsActive.HasValue)
            .WithMessage("At least one compatibility rule field must be provided.")
            .When(command => command.Model is not null);
        RuleFor(command => command.Model.Code).NotEmpty().MaximumLength(100)
            .When(command => command.Model?.Code is not null);
        RuleFor(command => command.Model.Name).NotEmpty().MaximumLength(200)
            .When(command => command.Model?.Name is not null);
        RuleFor(command => command.Model.Operator)
            .Must(value => Enum.TryParse<CompatibilityOperator>(value, true, out _))
            .WithMessage("Unsupported compatibility operator.")
            .When(command => command.Model?.Operator is not null);
        RuleFor(command => command.Model.FailureMessage).NotEmpty().MaximumLength(1000)
            .When(command => command.Model?.FailureMessage is not null);
        RuleFor(command => command.Model)
            .Must(model => !(model.FailureMessage is not null && model.ClearFailureMessage))
            .WithMessage("FailureMessage and ClearFailureMessage cannot be used together.")
            .When(command => command.Model is not null);
    }
}
