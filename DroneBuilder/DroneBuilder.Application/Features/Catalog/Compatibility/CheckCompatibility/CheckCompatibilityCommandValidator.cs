using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Compatibility.CheckCompatibility;

public sealed class CheckCompatibilityCommandValidator : AbstractValidator<CheckCompatibilityCommand>
{
    public CheckCompatibilityCommandValidator()
    {
        RuleFor(command => command.Model).NotNull();
        RuleFor(command => command.Model.LeftVariantId).NotEmpty()
            .When(command => command.Model is not null);
        RuleFor(command => command.Model.RightVariantId).NotEmpty()
            .When(command => command.Model is not null);
        RuleFor(command => command.Model)
            .Must(model => model.LeftVariantId != model.RightVariantId)
            .WithMessage("Two different variants are required.")
            .When(command => command.Model is not null);
    }
}
