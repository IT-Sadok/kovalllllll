using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Imports.GetProductExternalReferences;

public sealed class GetProductExternalReferencesQueryValidator
    : AbstractValidator<GetProductExternalReferencesQuery>
{
    public GetProductExternalReferencesQueryValidator()
    {
        RuleFor(query => query.ProductId).NotEmpty();
    }
}
