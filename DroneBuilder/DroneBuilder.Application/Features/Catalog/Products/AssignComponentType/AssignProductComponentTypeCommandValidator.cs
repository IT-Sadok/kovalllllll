using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Products.AssignComponentType;

public sealed class AssignProductComponentTypeCommandValidator
    : AbstractValidator<AssignProductComponentTypeCommand>
{
    public AssignProductComponentTypeCommandValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.Model).NotNull();
        RuleFor(command => command.Model.ComponentTypeId)
            .NotEmpty()
            .When(command => command.Model is not null);
    }
}
