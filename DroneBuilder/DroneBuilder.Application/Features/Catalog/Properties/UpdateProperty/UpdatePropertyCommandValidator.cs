using DroneBuilder.Domain.Entities;
using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Properties.UpdateProperty;

public class UpdatePropertyCommandValidator : AbstractValidator<UpdatePropertyCommand>
{
    public UpdatePropertyCommandValidator()
    {
        RuleFor(command => command.PropertyId).NotEmpty();
        RuleFor(command => command.Model).NotNull();
        When(command => command.Model is not null, () =>
        {
            RuleFor(command => command.Model)
                .Must(model => model.Code is not null || model.Name is not null || model.DataType is not null ||
                               model.UnitDefinitionId.HasValue || model.ClearUnitDefinition ||
                               model.IsFilterable.HasValue || model.IsCompatibilityRelevant.HasValue ||
                               model.AllowsMultipleValues.HasValue || model.Aliases is not null)
                .WithMessage("At least one property field must be provided.");
            RuleFor(command => command.Model.Code).NotEmpty().MaximumLength(100)
                .When(command => command.Model.Code is not null);
            RuleFor(command => command.Model.Name).NotEmpty().MaximumLength(100)
                .When(command => command.Model.Name is not null);
            RuleFor(command => command.Model.DataType)
                .Must(value => Enum.TryParse<SpecificationDataType>(value, true, out _))
                .WithMessage("Unsupported property data type.")
                .When(command => command.Model.DataType is not null);
            RuleFor(command => command.Model)
                .Must(model => !(model.UnitDefinitionId.HasValue && model.ClearUnitDefinition))
                .WithMessage("UnitDefinitionId and ClearUnitDefinition cannot be used together.");
            RuleForEach(command => command.Model.Aliases!).NotEmpty().MaximumLength(200)
                .When(command => command.Model.Aliases is not null);
        });
    }
}
