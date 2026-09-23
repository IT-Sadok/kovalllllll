using DroneBuilder.Application.Mediator.Queries.ValueQueries;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class GetValueByIdQueryValidator : AbstractValidator<GetValueByIdQuery>
{
    public GetValueByIdQueryValidator()
    {
        RuleFor(x => x.ValueId).NotEmpty().WithMessage("ValueId is required.");
    }
}

