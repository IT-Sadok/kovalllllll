using DroneBuilder.Application.Mediator.Queries.ImageQueries;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class GetImagesByProductIdQueryValidator : AbstractValidator<GetImagesByProductIdQuery>
{
    public GetImagesByProductIdQueryValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId is required.");
    }
}

