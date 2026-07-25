using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Properties.UpdateProperty;

public class UpdatePropertyCommandValidator : AbstractValidator<UpdatePropertyCommand>
{
    public UpdatePropertyCommandValidator()
    {
        RuleFor(x => x.PropertyId)
            .NotEmpty().WithMessage("Property ID is required.");

        RuleFor(x => x.Model)
            .NotNull().WithMessage("Update payload is required.")
            .Must(model => model?.Name is not null)
            .WithMessage("Property name must be provided.");

        When(x => x.Model != null, () =>
        {
            RuleFor(x => x.Model.Name)
                .MaximumLength(200).WithMessage("Property name must not exceed 200 characters.")
                .When(x => x.Model.Name != null);
        });
    }
}
