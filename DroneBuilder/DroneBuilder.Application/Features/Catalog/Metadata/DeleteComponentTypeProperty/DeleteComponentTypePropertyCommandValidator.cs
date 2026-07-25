using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Metadata.DeleteComponentTypeProperty;

public sealed class DeleteComponentTypePropertyCommandValidator
    : AbstractValidator<DeleteComponentTypePropertyCommand>
{
    public DeleteComponentTypePropertyCommandValidator()
    {
        RuleFor(command => command.ComponentTypeId).NotEmpty();
        RuleFor(command => command.PropertyId).NotEmpty();
    }
}
