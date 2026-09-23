using DroneBuilder.Application.Mediator.Queries.ImageQueries;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class GetImageByIdQueryValidator : AbstractValidator<GetImageByIdQuery>
{
    public GetImageByIdQueryValidator()
    {
        RuleFor(x => x.ImageId).NotEmpty().WithMessage("ImageId is required.");
    }
}

