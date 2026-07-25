using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Metadata.DeleteComponentType;

public sealed class DeleteComponentTypeCommandValidator : AbstractValidator<DeleteComponentTypeCommand>
{
    public DeleteComponentTypeCommandValidator()
    {
        RuleFor(command => command.ComponentTypeId).NotEmpty();
    }
}
