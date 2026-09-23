using FluentValidation;

namespace DroneBuilder.Application.Features.Values.DeleteValue;

public class DeleteValueCommandValidator : AbstractValidator<DeleteValueCommand>
{
    public DeleteValueCommandValidator()
    {
        RuleFor(x => x.ValueId).NotEmpty().WithMessage("ValueId is required.");
    }
}

