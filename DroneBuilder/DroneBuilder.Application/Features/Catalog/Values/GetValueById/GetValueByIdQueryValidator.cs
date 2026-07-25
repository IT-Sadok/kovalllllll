using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Values.GetValueById;

public class GetValueByIdQueryValidator : AbstractValidator<GetValueByIdQuery>
{
    public GetValueByIdQueryValidator()
    {
        RuleFor(x => x.PropertyId).NotEmpty().WithMessage("PropertyId is required.");
    }
}

