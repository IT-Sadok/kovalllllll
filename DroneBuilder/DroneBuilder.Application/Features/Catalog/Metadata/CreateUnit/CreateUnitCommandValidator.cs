using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Metadata.CreateUnit;

public sealed class CreateUnitCommandValidator : AbstractValidator<CreateUnitCommand>
{
    public CreateUnitCommandValidator()
    {
        RuleFor(command => command.Model).NotNull();
        RuleFor(command => command.Model.Code).NotEmpty().MaximumLength(50)
            .When(command => command.Model is not null);
        RuleFor(command => command.Model.Name).NotEmpty().MaximumLength(100)
            .When(command => command.Model is not null);
        RuleFor(command => command.Model.Symbol).NotEmpty().MaximumLength(30)
            .When(command => command.Model is not null);
        RuleFor(command => command.Model.Dimension).MaximumLength(100)
            .When(command => command.Model?.Dimension is not null);
        RuleFor(command => command.Model.ConversionFactorToBase).GreaterThan(0)
            .When(command => command.Model is not null);
        RuleForEach(command => command.Model.Aliases).NotEmpty().MaximumLength(100)
            .When(command => command.Model is not null);
    }
}
