using DroneBuilder.Domain.Entities;
using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Compatibility.CreateCompatibilityRule;

public sealed class CreateCompatibilityRuleCommandValidator
    : AbstractValidator<CreateCompatibilityRuleCommand>
{
    public CreateCompatibilityRuleCommandValidator()
    {
        RuleFor(command => command.Model).NotNull();
        RuleFor(command => command.Model.Code).NotEmpty().MaximumLength(100)
            .When(command => command.Model is not null);
        RuleFor(command => command.Model.Name).NotEmpty().MaximumLength(200)
            .When(command => command.Model is not null);
        RuleFor(command => command.Model.LeftComponentTypeId).NotEmpty()
            .When(command => command.Model is not null);
        RuleFor(command => command.Model.LeftPropertyId).NotEmpty()
            .When(command => command.Model is not null);
        RuleFor(command => command.Model.RightComponentTypeId).NotEmpty()
            .When(command => command.Model is not null);
        RuleFor(command => command.Model.RightPropertyId).NotEmpty()
            .When(command => command.Model is not null);
        RuleFor(command => command.Model.Operator)
            .Must(value => Enum.TryParse<CompatibilityOperator>(value, true, out _))
            .WithMessage("Unsupported compatibility operator.")
            .When(command => command.Model is not null);
        RuleFor(command => command.Model.FailureMessage).MaximumLength(1000)
            .When(command => command.Model?.FailureMessage is not null);
    }
}
