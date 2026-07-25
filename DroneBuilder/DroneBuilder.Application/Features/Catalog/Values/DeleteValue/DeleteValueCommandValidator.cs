using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Values.DeleteValue;

public class DeleteValueCommandValidator : AbstractValidator<DeleteValueCommand>
{
    public DeleteValueCommandValidator()
    {
        RuleFor(x => x.ValueId).NotEmpty().WithMessage("ValueId is required.");
    }
}

