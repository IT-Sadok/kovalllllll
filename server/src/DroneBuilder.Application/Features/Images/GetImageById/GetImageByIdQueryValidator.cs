using FluentValidation;

namespace DroneBuilder.Application.Features.Images.GetImageById;

public class GetImageByIdQueryValidator : AbstractValidator<GetImageByIdQuery>
{
    public GetImageByIdQueryValidator()
    {
        RuleFor(x => x.ImageId).NotEmpty().WithMessage("ImageId is required.");
    }
}

