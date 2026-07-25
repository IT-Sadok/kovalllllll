using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.ProductVariants.CreateProductVariant;

public sealed class CreateProductVariantCommandValidator : AbstractValidator<CreateProductVariantCommand>
{
    public CreateProductVariantCommandValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.Model).NotNull();
        RuleFor(command => command.Model.Sku).NotEmpty().MaximumLength(100)
            .When(command => command.Model is not null);
        RuleFor(command => command.Model.Name).MaximumLength(200)
            .When(command => command.Model?.Name is not null);
        RuleFor(command => command.Model.Price).GreaterThanOrEqualTo(0)
            .When(command => command.Model is not null);
        RuleFor(command => command.Model.CurrencyCode).Length(3)
            .When(command => command.Model is not null);
    }
}
