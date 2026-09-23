using FluentValidation;

namespace DroneBuilder.Application.Features.Values.UpdateValue;

public class UpdateValueCommandValidator : AbstractValidator<UpdateValueCommand>
{
    public UpdateValueCommandValidator()
    {
        RuleFor(x => x.ValueId)
            .NotEmpty().WithMessage("Value ID is required.");

        RuleFor(x => x.Model)
            .NotNull().WithMessage("Value data is required.");

        When(x => x.Model != null, () =>
        {
            RuleFor(x => x.Model.Text)
                .NotEmpty().WithMessage("Value text must not be empty.")
                .MaximumLength(100).WithMessage("Value text must not exceed 100 characters.")
                .When(x => x.Model.Text != null);
        });
    }
}
