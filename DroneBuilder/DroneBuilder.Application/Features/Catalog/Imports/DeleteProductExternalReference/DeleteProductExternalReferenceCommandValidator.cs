using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Imports.DeleteProductExternalReference;

public sealed class DeleteProductExternalReferenceCommandValidator
    : AbstractValidator<DeleteProductExternalReferenceCommand>
{
    public DeleteProductExternalReferenceCommandValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.ReferenceId).NotEmpty();
    }
}
