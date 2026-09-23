using FluentValidation;

namespace DroneBuilder.Application.Features.Properties.GetPropertyById;

public class GetPropertyByIdQueryValidator : AbstractValidator<GetPropertyByIdQuery>
{
    public GetPropertyByIdQueryValidator()
    {
        RuleFor(x => x.PropertyId).NotEmpty().WithMessage("PropertyId is required.");
    }
}

