using FluentValidation;

namespace DroneBuilder.Application.Features.Images.SetPrimaryImage;

public class SetPrimaryImageCommandValidator : AbstractValidator<SetPrimaryImageCommand>
{
    public SetPrimaryImageCommandValidator()
    {
        RuleFor(x => x.ImageId).NotEmpty().WithMessage("ImageId is required.");
    }
}

