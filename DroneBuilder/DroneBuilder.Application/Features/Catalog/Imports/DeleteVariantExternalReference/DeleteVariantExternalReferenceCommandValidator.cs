using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Imports.DeleteVariantExternalReference;

public sealed class DeleteVariantExternalReferenceCommandValidator
    : AbstractValidator<DeleteVariantExternalReferenceCommand>
{
    public DeleteVariantExternalReferenceCommandValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.VariantId).NotEmpty();
        RuleFor(command => command.ReferenceId).NotEmpty();
    }
}
