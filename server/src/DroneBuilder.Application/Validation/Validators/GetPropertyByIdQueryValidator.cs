using DroneBuilder.Application.Mediator.Queries.PropertyQueries;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class GetPropertyByIdQueryValidator : AbstractValidator<GetPropertyByIdQuery>
{
    public GetPropertyByIdQueryValidator()
    {
        RuleFor(x => x.PropertyId).NotEmpty().WithMessage("PropertyId is required.");
    }
}

