using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Metadata.UpdateUnit;

public sealed class UpdateUnitCommandValidator : AbstractValidator<UpdateUnitCommand>
{
    public UpdateUnitCommandValidator()
    {
        RuleFor(command => command.UnitId).NotEmpty();
        RuleFor(command => command.Model).NotNull();
        RuleFor(command => command.Model)
            .Must(model => model.Code is not null || model.Name is not null || model.Symbol is not null ||
                           model.Dimension is not null || model.ClearDimension ||
                           model.ConversionFactorToBase.HasValue || model.Aliases is not null)
            .WithMessage("At least one unit field must be provided.")
            .When(command => command.Model is not null);
        RuleFor(command => command.Model.Code).NotEmpty().MaximumLength(50)
            .When(command => command.Model?.Code is not null);
        RuleFor(command => command.Model.Name).NotEmpty().MaximumLength(100)
            .When(command => command.Model?.Name is not null);
        RuleFor(command => command.Model.Symbol).NotEmpty().MaximumLength(30)
            .When(command => command.Model?.Symbol is not null);
        RuleFor(command => command.Model.Dimension).NotEmpty().MaximumLength(100)
            .When(command => command.Model?.Dimension is not null);
        RuleFor(command => command.Model.ConversionFactorToBase).GreaterThan(0)
            .When(command => command.Model?.ConversionFactorToBase is not null);
        RuleFor(command => command.Model)
            .Must(model => !(model.ClearDimension && model.Dimension is not null))
            .WithMessage("Dimension and ClearDimension cannot be used together.")
            .When(command => command.Model is not null);
    }
}
