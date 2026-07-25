using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Metadata.GetAdminComponentType;

public sealed class GetAdminComponentTypeQueryValidator : AbstractValidator<GetAdminComponentTypeQuery>
{
    public GetAdminComponentTypeQueryValidator()
    {
        RuleFor(query => query.ComponentTypeId).NotEmpty();
    }
}
