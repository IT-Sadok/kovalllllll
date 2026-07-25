using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Images.GetImagesByProductId;

public class GetImagesByProductIdQueryValidator : AbstractValidator<GetImagesByProductIdQuery>
{
    public GetImagesByProductIdQueryValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId is required.");
    }
}

