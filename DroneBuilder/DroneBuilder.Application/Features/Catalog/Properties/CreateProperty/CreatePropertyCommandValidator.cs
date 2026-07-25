using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Properties.CreateProperty;

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
                .MaximumLength(200).WithMessage("Property name must not exceed 200 characters.");
        });
    }
}
