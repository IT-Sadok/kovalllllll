using FluentValidation;

namespace DroneBuilder.Application.Features.Products.SetProductAttributes;

public class SetProductAttributesCommandValidator : AbstractValidator<SetProductAttributesCommand>
{
    public SetProductAttributesCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId is required.");

        RuleFor(x => x.Attributes)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Attributes are required.")
            .Must(a => a.Count <= 50).WithMessage("A product can have at most 50 attributes.")
            .Must(a => a.Select(x => x.Name?.Trim().ToLowerInvariant()).Distinct().Count() == a.Count)
            .WithMessage("Attribute names must be unique.");

        RuleForEach(x => x.Attributes).ChildRules(attribute =>
        {
            attribute.RuleFor(a => a.Name)
                .NotEmpty().WithMessage("Attribute name is required.")
                .MaximumLength(100).WithMessage("Attribute name must not exceed 100 characters.");

            attribute.RuleFor(a => a.Value)
                .NotEmpty().WithMessage("Attribute value is required.")
                .MaximumLength(500).WithMessage("Attribute value must not exceed 500 characters.");
        });
    }
}
