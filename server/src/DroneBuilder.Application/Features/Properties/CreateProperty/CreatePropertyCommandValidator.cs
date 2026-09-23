using FluentValidation;

namespace DroneBuilder.Application.Features.Properties.CreateProperty;

public class CreatePropertyCommandValidator : AbstractValidator<CreatePropertyCommand>
{
    public CreatePropertyCommandValidator()
    {
        RuleFor(x => x.Model)
            .NotNull().WithMessage("Property data is required.");

        When(x => x.Model != null, () =>
        {
            RuleFor(x => x.Model.Name)
                .NotEmpty().WithMessage("Property name is required.")
                .MaximumLength(100).WithMessage("Property name must not exceed 100 characters.");

            RuleForEach(x => x.Model.Values)
                .ChildRules(value => value.RuleFor(v => v.Text)
                    .NotEmpty().WithMessage("Value text is required.")
                    .MaximumLength(100).WithMessage("Value text must not exceed 100 characters."));
        });
    }
}
