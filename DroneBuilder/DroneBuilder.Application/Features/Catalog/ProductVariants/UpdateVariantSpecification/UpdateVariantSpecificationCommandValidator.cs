using DroneBuilder.Application.Features.Catalog.ProductSpecifications;
using DroneBuilder.Application.Features.Catalog.ProductVariants.Models;
using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.ProductVariants.UpdateVariantSpecification;

public sealed class UpdateVariantSpecificationCommandValidator
    : AbstractValidator<UpdateVariantSpecificationCommand>
{
    public UpdateVariantSpecificationCommandValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.VariantId).NotEmpty();
        RuleFor(command => command.SpecificationId).NotEmpty();
        RuleFor(command => command.Model).NotNull();
        RuleFor(command => command.Model)
            .SetValidator(new SpecificationValueInputValidator<UpdateProductVariantSpecificationModel>())
            .When(command => command.Model is not null);
    }
}
