using DroneBuilder.Application.Features.Catalog.ProductSpecifications;
using DroneBuilder.Application.Features.Catalog.ProductVariants.Models;
using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.ProductVariants.CreateVariantSpecification;

public sealed class CreateVariantSpecificationCommandValidator
    : AbstractValidator<CreateVariantSpecificationCommand>
{
    public CreateVariantSpecificationCommandValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.VariantId).NotEmpty();
        RuleFor(command => command.Model).NotNull();
        RuleFor(command => command.Model.PropertyId).NotEmpty()
            .When(command => command.Model is not null);
        RuleFor(command => command.Model)
            .SetValidator(new SpecificationValueInputValidator<CreateProductVariantSpecificationModel>())
            .When(command => command.Model is not null);
    }
}
