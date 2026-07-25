using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Properties.RemoveValueFromProperty;

public class RemoveValueFromPropertyCommandValidator : AbstractValidator<RemoveValueFromPropertyCommand>
{
    public RemoveValueFromPropertyCommandValidator()
    {
        RuleFor(x => x.PropertyId).NotEmpty().WithMessage("PropertyId is required.");
        RuleFor(x => x.ValueId).NotEmpty().WithMessage("ValueId is required.");
    }
}

