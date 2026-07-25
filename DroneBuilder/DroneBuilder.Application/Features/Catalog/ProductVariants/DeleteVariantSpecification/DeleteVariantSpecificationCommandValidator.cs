using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.ProductVariants.DeleteVariantSpecification;

public sealed class DeleteVariantSpecificationCommandValidator
    : AbstractValidator<DeleteVariantSpecificationCommand>
{
    public DeleteVariantSpecificationCommandValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.VariantId).NotEmpty();
        RuleFor(command => command.SpecificationId).NotEmpty();
    }
}
