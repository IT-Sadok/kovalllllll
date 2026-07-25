using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.ProductVariants.UpdateProductVariant;

public sealed class UpdateProductVariantCommandValidator : AbstractValidator<UpdateProductVariantCommand>
{
    public UpdateProductVariantCommandValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.VariantId).NotEmpty();
        RuleFor(command => command.Model).NotNull();
        RuleFor(command => command.Model)
            .Must(model => model.Sku is not null || model.Name is not null || model.Price.HasValue ||
                           model.CurrencyCode is not null || model.IsActive.HasValue || model.IsDefault.HasValue)
            .WithMessage("At least one variant field must be provided.")
            .When(command => command.Model is not null);
        RuleFor(command => command.Model.Sku).NotEmpty().MaximumLength(100)
            .When(command => command.Model?.Sku is not null);
        RuleFor(command => command.Model.Name).MaximumLength(200)
            .When(command => command.Model?.Name is not null);
        RuleFor(command => command.Model.Price).GreaterThanOrEqualTo(0)
            .When(command => command.Model?.Price is not null);
        RuleFor(command => command.Model.CurrencyCode).Length(3)
            .When(command => command.Model?.CurrencyCode is not null);
    }
}
