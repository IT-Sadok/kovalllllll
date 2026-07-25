using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Products.GetPropertiesByProductId;

public class GetPropertiesByProductIdQueryValidator : AbstractValidator<GetPropertiesByProductIdQuery>
{
    public GetPropertiesByProductIdQueryValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId is required.");
    }
}

