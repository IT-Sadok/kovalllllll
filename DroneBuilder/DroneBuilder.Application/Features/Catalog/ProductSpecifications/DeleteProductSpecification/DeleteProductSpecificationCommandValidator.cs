using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.ProductSpecifications.DeleteProductSpecification;

public sealed class DeleteProductSpecificationCommandValidator
    : AbstractValidator<DeleteProductSpecificationCommand>
{
    public DeleteProductSpecificationCommandValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.SpecificationId).NotEmpty();
    }
}
