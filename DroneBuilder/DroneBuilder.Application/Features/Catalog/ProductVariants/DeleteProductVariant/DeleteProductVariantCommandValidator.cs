using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.ProductVariants.DeleteProductVariant;

public sealed class DeleteProductVariantCommandValidator : AbstractValidator<DeleteProductVariantCommand>
{
    public DeleteProductVariantCommandValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.VariantId).NotEmpty();
    }
}
