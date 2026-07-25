using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Imports.GetVariantExternalReferences;

public sealed class GetVariantExternalReferencesQueryValidator
    : AbstractValidator<GetVariantExternalReferencesQuery>
{
    public GetVariantExternalReferencesQueryValidator()
    {
        RuleFor(query => query.ProductId).NotEmpty();
        RuleFor(query => query.VariantId).NotEmpty();
    }
}
