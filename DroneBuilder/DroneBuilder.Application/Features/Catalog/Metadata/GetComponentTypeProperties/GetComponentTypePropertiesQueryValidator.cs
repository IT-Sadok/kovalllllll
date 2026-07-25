using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Metadata.GetComponentTypeProperties;

public sealed class GetComponentTypePropertiesQueryValidator
    : AbstractValidator<GetComponentTypePropertiesQuery>
{
    public GetComponentTypePropertiesQueryValidator()
    {
        RuleFor(query => query.ComponentTypeId).NotEmpty();
    }
}
