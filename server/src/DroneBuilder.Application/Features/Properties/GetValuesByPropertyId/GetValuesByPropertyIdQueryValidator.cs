using FluentValidation;

namespace DroneBuilder.Application.Features.Properties.GetValuesByPropertyId;

public class GetValuesByPropertyIdQueryValidator : AbstractValidator<GetValuesByPropertyIdQuery>
{
    public GetValuesByPropertyIdQueryValidator()
    {
        RuleFor(x => x.PropertyId).NotEmpty().WithMessage("PropertyId is required.");
    }
}

