using DroneBuilder.Application.Mediator.Queries.ProductQueries;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class GetProductByIdQueryValidator : AbstractValidator<GetProductByIdQuery>
{
    public GetProductByIdQueryValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId is required.");
    }
}

