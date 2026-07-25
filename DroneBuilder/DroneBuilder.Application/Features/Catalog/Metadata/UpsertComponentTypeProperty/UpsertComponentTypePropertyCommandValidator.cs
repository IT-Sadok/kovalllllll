using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Metadata.UpsertComponentTypeProperty;

public sealed class UpsertComponentTypePropertyCommandValidator
    : AbstractValidator<UpsertComponentTypePropertyCommand>
{
    public UpsertComponentTypePropertyCommandValidator()
    {
        RuleFor(command => command.ComponentTypeId).NotEmpty();
        RuleFor(command => command.PropertyId).NotEmpty();
        RuleFor(command => command.Model).NotNull();
        RuleFor(command => command.Model.SortOrder).GreaterThanOrEqualTo(0)
            .When(command => command.Model is not null);
        RuleFor(command => command.Model.PropertyId)
            .Must((command, propertyId) => propertyId == Guid.Empty || propertyId == command.PropertyId)
            .WithMessage("PropertyId in the payload must match the route.")
            .When(command => command.Model is not null);
    }
}
