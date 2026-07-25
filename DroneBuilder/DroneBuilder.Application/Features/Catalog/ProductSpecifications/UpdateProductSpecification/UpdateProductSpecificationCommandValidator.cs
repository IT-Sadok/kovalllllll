using DroneBuilder.Application.Features.Catalog.ProductSpecifications.Models;
using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.ProductSpecifications.UpdateProductSpecification;

public sealed class UpdateProductSpecificationCommandValidator
    : AbstractValidator<UpdateProductSpecificationCommand>
{
    public UpdateProductSpecificationCommandValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.SpecificationId).NotEmpty();
        RuleFor(command => command.Model).NotNull();
        RuleFor(command => command.Model)
            .SetValidator(new SpecificationValueInputValidator<UpdateProductSpecificationModel>())
            .When(command => command.Model is not null);
    }
}
