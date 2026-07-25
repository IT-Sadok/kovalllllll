using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Imports.UpsertVariantExternalReference;

public sealed class UpsertVariantExternalReferenceCommandValidator
    : AbstractValidator<UpsertVariantExternalReferenceCommand>
{
    public UpsertVariantExternalReferenceCommandValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.VariantId).NotEmpty();
        RuleFor(command => command.SourceId).NotEmpty();
        RuleFor(command => command.Model).NotNull();
        RuleFor(command => command.Model.ExternalId).NotEmpty().MaximumLength(300)
            .When(command => command.Model is not null);
        RuleFor(command => command.Model.SourceUrl).MaximumLength(1000)
            .When(command => command.Model?.SourceUrl is not null);
        RuleFor(command => command.Model.ContentHash).MaximumLength(128)
            .When(command => command.Model?.ContentHash is not null);
    }
}
