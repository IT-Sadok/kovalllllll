using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Values.UpdateValue;

public class UpdateValueCommandValidator : AbstractValidator<UpdateValueCommand>
{
    public UpdateValueCommandValidator()
    {
        RuleFor(x => x.ValueId)
            .NotEmpty().WithMessage("Value ID is required.");

        RuleFor(x => x.Model)
            .NotNull().WithMessage("Update payload is required.")
            .Must(model => model?.Text is not null)
            .WithMessage("Value text must be provided.");

        When(x => x.Model != null, () =>
        {
            RuleFor(x => x.Model.Text)
                .MaximumLength(500).WithMessage("Value text must not exceed 500 characters.")
                .When(x => x.Model.Text != null);
        });
    }
}
