using DroneBuilder.Application.Mediator.Queries.PropertyQueries;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class GetValuesByPropertyIdQueryValidator : AbstractValidator<GetValuesByPropertyIdQuery>
{
    public GetValuesByPropertyIdQueryValidator()
    {
        RuleFor(x => x.PropertyId).NotEmpty().WithMessage("PropertyId is required.");
    }
}

