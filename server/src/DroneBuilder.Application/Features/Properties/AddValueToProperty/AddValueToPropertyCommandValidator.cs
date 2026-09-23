using FluentValidation;

namespace DroneBuilder.Application.Features.Properties.AddValueToProperty;

public class AddValueToPropertyCommandValidator : AbstractValidator<AddValueToPropertyCommand>
{
    public AddValueToPropertyCommandValidator()
    {
        RuleFor(x => x.PropertyId).NotEmpty().WithMessage("PropertyId is required.");
        RuleFor(x => x.ValueId).NotEmpty().WithMessage("ValueId is required.");
    }
}

