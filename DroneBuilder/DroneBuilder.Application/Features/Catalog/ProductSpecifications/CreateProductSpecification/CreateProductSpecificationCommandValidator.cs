using DroneBuilder.Application.Features.Catalog.ProductSpecifications.Models;
using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.ProductSpecifications.CreateProductSpecification;

public sealed class CreateProductSpecificationCommandValidator
    : AbstractValidator<CreateProductSpecificationCommand>
{
    public CreateProductSpecificationCommandValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.Model).NotNull();
        RuleFor(command => command.Model.PropertyId)
            .NotEmpty()
            .When(command => command.Model is not null);
        RuleFor(command => command.Model)
            .SetValidator(new SpecificationValueInputValidator<CreateProductSpecificationModel>())
            .When(command => command.Model is not null);
    }
}
