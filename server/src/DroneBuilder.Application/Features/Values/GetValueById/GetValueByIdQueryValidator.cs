using FluentValidation;

namespace DroneBuilder.Application.Features.Values.GetValueById;

public class GetValueByIdQueryValidator : AbstractValidator<GetValueByIdQuery>
{
    public GetValueByIdQueryValidator()
    {
        RuleFor(x => x.ValueId).NotEmpty().WithMessage("ValueId is required.");
    }
}

